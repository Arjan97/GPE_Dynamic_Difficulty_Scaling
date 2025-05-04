using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Rigidbody))]
public class InfiniteRunnerMovement : MonoBehaviour
{
    public static InfiniteRunnerMovement Instance { get; private set; }

    [Header("Camera Rotator Clamp")]
    public float clampMin = -160f;
    public float clampMax = 160f;

    [Header("Movement Settings")]
    public float forwardSpeed = 5f;
    public float rotatingSpeed = 5f; // Updated over time
    public float matchPlaneRotationSpeed = 10f;

    [Header("Player Turning")]
    [SerializeField] float defaultYaw = 90f;      // Default Y rotation (facing forward)
    [SerializeField] float turnOffset = 20f;      // How many degrees to offset on full input
    [SerializeField] float turnSpeed = 10f;       // Speed to interpolate to the target

    [Header("Speed Scaling Over Time")]
    [SerializeField] float minForwardSpeed = 30f;
    [SerializeField] float maxForwardSpeed = 90f;
    [SerializeField] float accelerationDuration = 90f;
    float playTime = 0f;

    [Header("Rotation Speed Scaling")]
    [SerializeField] float minRotatingSpeed = 5f;
    [SerializeField] float maxRotatingSpeed = 15f;

    [Header("Tilt Controls (Mobile)")]
    public float tiltSensitivity = 2f;
    [SerializeField] float tiltDeadzone = 0.1f;
    [SerializeField] InputAction tiltAction;
    [SerializeField] float mobileRotationMultiplier = 2f;

    [Header("Joystick Movement Action")]
    // This is the separate action for joystick movement. Set this in your inspector to the "Move" action
    // on your "Joystick" action map.
    [SerializeField] InputActionReference joystickMoveActionRef;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    [SerializeField] float minSwipeDistance = 100f;
    [Tooltip("Force applied when swiping down (slam move).")]
    [SerializeField] float downForce = 10f;
    [SerializeField] float coyoteTime = 0.2f;
    float coyoteTimer = 0f;
    bool hasJumped = false;
    bool wasGroundedLastFrame = false;

    Rigidbody rb;
    // This value comes from the joystick move action.
    Vector2 moveInput = Vector2.zero;
    float tiltInput = 0f;
    bool isGrounded;
    SphereCollider groundChecker;
    Transform lastGroundHit;
    PlayerAnimatorController animController;
    AudioSource audioSource;
    Vector2 touchStartPos;
    [SerializeField] private ChaseScript chaserScript;
    bool gameOver = false;
    [SerializeField] InputActionReference slamAction;

    float _horizontalInput = 0f;
    // Track the touch id used by the joystick (from the separate action).
    int joystickTouchId = -1;

    // The HorizontalInput property now depends solely on moveInput updated via the joystick action
    public float HorizontalInput
    {
        get
        {
            if (Application.isMobilePlatform)
            {
                return SettingsManager.Instance.JoystickEnabled ? moveInput.x : tiltInput;
            }
            else
            {
                return moveInput.x;
            }
        }
    }

    void Awake()
    {
        if (Application.isMobilePlatform)
        {
            if (Accelerometer.current != null && !Accelerometer.current.enabled)
                InputSystem.EnableDevice(Accelerometer.current);
            EnhancedTouchSupport.Enable();
        }
        Instance = this;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        groundChecker = groundCheck.GetComponent<SphereCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        animController = GameObject.FindGameObjectWithTag("PlayerAnim").GetComponent<PlayerAnimatorController>();

        // Enable and subscribe to the joystick movement action in its own action map.
        if (joystickMoveActionRef != null)
        {
            joystickMoveActionRef.action.performed += OnJoystickMove;
            joystickMoveActionRef.action.canceled += OnJoystickMoveCanceled;
            joystickMoveActionRef.action.Enable();
        }
    }

    private void OnDestroy()
    {
        if (joystickMoveActionRef != null)
        {
            joystickMoveActionRef.action.performed -= OnJoystickMove;
            joystickMoveActionRef.action.canceled -= OnJoystickMoveCanceled;
        }
    }

    // This callback updates moveInput independently from swipe.
    private void OnJoystickMove(InputAction.CallbackContext context)
    {
        // Update the joystick move input directly.
        moveInput = context.ReadValue<Vector2>();

        // Optionally, record the touch id if it's a touch-based control.
        // For simplicity, assume the first active touch is the joystick.
        var touches = Touch.activeTouches;
        if (touches.Count > 0)
            joystickTouchId = touches[0].touchId;
    }

    private void OnJoystickMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
        joystickTouchId = -1;
    }

    private void Slam()
    {
        rb.AddForce(-transform.up * downForce, ForceMode.Impulse);
    }

    void Update()
    {
        if (gameOver)
            return;

        UpdateSpeedOverTime();

        // Capture previous grounded state
        bool wasGroundedPreviously = isGrounded;
        CheckGround();

        if (!isGrounded)
            coyoteTimer = Mathf.Max(coyoteTimer - Time.deltaTime, 0f);

        // If just landed, reset jump flag.
        if (isGrounded && !wasGroundedPreviously)
        {
            hasJumped = false;
            coyoteTimer = coyoteTime;
        }

        float targetHorizontal = 0f;

        // For mobile, if joystick is disabled, fallback to tilt controls.
        if (Application.isMobilePlatform)
        {
            HandleSwipeJump();
            if (SettingsManager.Instance.JoystickEnabled)
            {
                targetHorizontal = moveInput.x;
            }
            else
            {
                Vector3 tilt = tiltAction.ReadValue<Vector3>();
                float rawTilt = tilt.x * tiltSensitivity;
                targetHorizontal = (Mathf.Abs(rawTilt) > tiltDeadzone) ? Mathf.Clamp(rawTilt, -1f, 1f) : 0f;
                tiltInput = targetHorizontal;
            }
        }
        else
        {
            targetHorizontal = moveInput.x;
            if (slamAction.action.WasPressedThisFrame())
                Slam();
        }

        _horizontalInput = targetHorizontal;
    }

    void FixedUpdate()
    {
        if (gameOver)
            return;

        Move(_horizontalInput);
        ApplyPlayerTurning(_horizontalInput);
    }

    void TriggerJump()
    {
        if (!CanJump())
            return;

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        hasJumped = true;
        ResetCoyote();

        if (audioSource != null)
            audioSource.Play();
        if (chaserScript != null)
            chaserScript.Jump();
    }

    void CheckGround()
    {
        Collider[] colliders = new Collider[3];
        int hitCount = Physics.OverlapSphereNonAlloc(groundCheck.position, groundChecker.radius, colliders, groundLayer);

        bool previousGroundedState = isGrounded;
        isGrounded = false;
        animController?.SetGroundedState(false);

        for (int i = 0; i < hitCount; i++)
        {
            if (colliders[i] != null && colliders[i].gameObject != gameObject)
            {
                isGrounded = true;
                lastGroundHit = colliders[i].transform;
                AlignToGround(colliders[i].transform.up);
                animController?.SetGroundedState(true);
                break;
            }
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (!previousGroundedState)
                hasJumped = false;
            chaserScript?.SetFalling(false);
        }
        else
        {
            chaserScript?.SetFalling(true);
        }
    }

    void UpdateSpeedOverTime()
    {
        playTime += Time.deltaTime;
        float t = Mathf.Clamp01(playTime / accelerationDuration);
        forwardSpeed = Mathf.Lerp(minForwardSpeed, maxForwardSpeed, t);
        rotatingSpeed = Mathf.Lerp(minRotatingSpeed, maxRotatingSpeed, t);
    }

    void ApplyPlayerTurning(float horizontalInput)
    {
        float targetYaw = defaultYaw + horizontalInput * turnOffset;
        float currentYaw = transform.localEulerAngles.y;
        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * turnSpeed);
        Vector3 newEuler = new Vector3(transform.localEulerAngles.x, newYaw, transform.localEulerAngles.z);
        transform.localRotation = Quaternion.Euler(newEuler);
    }

    void Move(float horizontalInput)
    {
        GameObject[] tunnels = GameObject.FindGameObjectsWithTag("Center");
        if (tunnels != null && tunnels.Length > 0)
        {
            float rotationBoost = 1f;
            if (Application.isMobilePlatform)
                rotationBoost = mobileRotationMultiplier;

            float rotationAmount = -horizontalInput * rotatingSpeed * rotationBoost * Time.deltaTime;
            foreach (GameObject tunnel in tunnels)
            {
                tunnel.transform.Rotate(rotationAmount, 0, 0);
                tunnel.transform.Translate(Vector3.left * forwardSpeed * Time.deltaTime, Space.World);
            }
        }

        GameObject camRotator = GameObject.FindGameObjectWithTag("Rotate");
        if (camRotator != null)
        {
            camRotator.transform.Rotate(0, 0, -horizontalInput * rotatingSpeed * Time.deltaTime);
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0)
        {
            hasJumped = false;
            coyoteTimer = coyoteTime;
        }
    }


    // The OnMove function is no longer used for the joystick because we use the separate action callback.
    // However, you might keep it if you want fallback behavior.
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private bool CanJump() => coyoteTimer > 0f && !hasJumped;

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            TriggerJump();
    }

    private void ResetCoyote()
    {
        coyoteTimer = 0f;
    }

    void HandleSwipeJump()
    {
        // Process Enhanced Touch for swipe gestures.
        if (Touch.activeTouches.Count == 0)
            return;

        foreach (var touch in Touch.activeTouches)
        {
            // Ignore the touch used by the joystick.
            if (touch.touchId == joystickTouchId)
                continue;

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStartPos = touch.screenPosition;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                Vector2 end = touch.screenPosition;
                float swipeY = end.y - touchStartPos.y;

                if (swipeY > minSwipeDistance)
                    TriggerJump();
                else if (swipeY < -minSwipeDistance)
                    rb.AddForce(-transform.up * downForce, ForceMode.Impulse);
            }
        }
    }

    public void SetGameOver(bool state) => gameOver = state;
    public bool IsGameOver() => gameOver;

    void AlignToGround(Vector3 groundNormal)
    {
        Quaternion upAlignedRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
        float targetZRotation = lastGroundHit != null ? lastGroundHit.eulerAngles.z : transform.eulerAngles.z;
        Vector3 targetEuler = upAlignedRotation.eulerAngles;
        targetEuler.z = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, matchPlaneRotationSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, matchPlaneRotationSpeed * Time.deltaTime);
    }

    public void SetForwardSpeed(float speed)
    {
        forwardSpeed = speed;
    }

    public void SetRotatingSpeed(float speed)
    {
        rotatingSpeed = speed;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            SphereCollider sc = groundChecker != null ? groundChecker : groundCheck.GetComponent<SphereCollider>();
            if (sc != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, sc.radius);
            }
        }
    }

    void OnEnable()
    {
        tiltAction.Enable();
        slamAction.action.Enable();
        if (joystickMoveActionRef != null)
        {
            joystickMoveActionRef.action.Enable();
        }
    }

    void OnDisable()
    {
        tiltAction.Disable();
        slamAction.action.Disable();
        if (joystickMoveActionRef != null)
        {
            joystickMoveActionRef.action.Disable();
        }
    }
}
