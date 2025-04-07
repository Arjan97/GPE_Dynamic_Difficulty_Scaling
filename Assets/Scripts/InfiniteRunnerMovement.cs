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
    public float rotatingSpeed = 5f;
    public float matchPlaneRotationSpeed = 10f;

    [Header("Player Turning")]
    [SerializeField] float defaultYaw = 90f;   // Default Y rotation (facing forward)
    [SerializeField] float turnOffset = 20f;   // How many degrees to offset on full input
    [SerializeField] float turnSpeed = 10f;      // Speed to interpolate to the target

    [Header("Speed Scaling Over Time")]
    [SerializeField] float minForwardSpeed = 30f;
    [SerializeField] float maxForwardSpeed = 90f;
    [SerializeField] float accelerationDuration = 90f;
    float playTime = 0f;


    [Header("Tilt Controls (Mobile)")]
    public float tiltSensitivity = 2f;
    [SerializeField] float tiltDeadzone = 0.1f;
    [SerializeField] InputAction tiltAction;
    [SerializeField] float mobileRotationMultiplier = 2f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    [SerializeField] float minSwipeDistance = 100f;
    [Tooltip("Force applied when swiping down (slam move).")]
    [SerializeField] float downForce = 10f;

    Rigidbody rb;
    Vector2 moveInput;
    float tiltInput;
    bool isGrounded;
    SphereCollider groundChecker;
    Transform lastGroundHit;
    PlayerAnimatorController animController;
    private AudioSource audioSource;
    Vector2 touchStartPos;
    [SerializeField] private ChaseScript chaserScript;
    bool gameOver = false;
    public float HorizontalInput
    {
        get
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return tiltInput;
#else
            return moveInput.x;
#endif
        }
    }

    void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Accelerometer.current != null && !Accelerometer.current.enabled)
            InputSystem.EnableDevice(Accelerometer.current);
        EnhancedTouchSupport.Enable();
#endif
        Instance = this;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        groundChecker = groundCheck.GetComponent<SphereCollider>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        animController = GameObject.FindGameObjectWithTag("PlayerAnim").GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        if (!gameOver) {
            CheckGround();

            UpdateForwardSpeedOverTime();

#if UNITY_ANDROID && !UNITY_EDITOR
if (!gameOver){
        HandleSwipeJump();

}
#endif
        }

    }

    void FixedUpdate()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        if (!gameOver)
        {

            Move(HorizontalInput);
            ApplyPlayerTurning(HorizontalInput);
        }

#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        Vector3 tilt = tiltAction.ReadValue<Vector3>();
        float rawInput = tilt.x * tiltSensitivity;
        float horizontal = Mathf.Abs(rawInput) > tiltDeadzone ? Mathf.Clamp(rawInput, -1f, 1f) : 0f;

        tiltInput = horizontal;
        if (!gameOver){
        
        Move(horizontal);
    ApplyPlayerTurning(tiltInput);

        }
#endif
    }
    void UpdateForwardSpeedOverTime()
    {
        playTime += Time.deltaTime;
        float t = Mathf.Clamp01(playTime / accelerationDuration);
        forwardSpeed = Mathf.Lerp(minForwardSpeed, maxForwardSpeed, t);
    }
    void ApplyPlayerTurning(float horizontalInput)
    {
        // Calculate target yaw based on input.
        // For horizontalInput of -1, targetYaw = 90 - 20 = 70.
        // For horizontalInput of  0, targetYaw = 90.
        // For horizontalInput of  1, targetYaw = 90 + 20 = 110.
        float targetYaw = defaultYaw + horizontalInput * turnOffset;

        // Get the current local Y rotation.
        float currentYaw = transform.localEulerAngles.y;
        // Use LerpAngle to smoothly interpolate and handle wrap-around (0°/360°).
        float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * turnSpeed);

        // Apply the new yaw while preserving the current X and Z rotations.
        Vector3 newEuler = new Vector3(transform.localEulerAngles.x, newYaw, transform.localEulerAngles.z);
        transform.localRotation = Quaternion.Euler(newEuler);
    }
    void HandleSwipeJump()
    {
        if (Touch.activeTouches.Count == 0)
            return;

        foreach (var touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStartPos = touch.screenPosition;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
            {
                Vector2 end = touch.screenPosition;
                float swipeY = end.y - touchStartPos.y;

                // Swipe up = Jump
                if (swipeY > minSwipeDistance && isGrounded)
                {
                    rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
                    if (audioSource != null)
                        audioSource.Play();
                    // Make the chaser jump too
                    if (chaserScript != null)
                    {
                        chaserScript.Jump();
                    }
                }

                // Swipe down = Slam
                else if (swipeY < -minSwipeDistance)
                {
                    rb.AddForce(-transform.up * downForce, ForceMode.Impulse);
                }
            }
        }
    }
    public bool GameOver(bool gameOverBool)
    {
      return gameOver = gameOverBool;
    }
    void CheckGround()
    {
        Collider[] colliders = new Collider[3];
        int hitCount = Physics.OverlapSphereNonAlloc(groundCheck.position, groundChecker.radius, colliders, groundLayer);
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
        if (!isGrounded)
        {
            // If we are not grounded => falling
            if (chaserScript != null)
                chaserScript.SetFalling(true);
        }
        else
        {
            if (chaserScript != null)
                chaserScript.SetFalling(false);
        }
    }

    void AlignToGround(Vector3 groundNormal)
    {
        Quaternion upAlignedRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;
        float targetZRotation = lastGroundHit != null ? lastGroundHit.eulerAngles.z : transform.eulerAngles.z;
        Vector3 targetEuler = upAlignedRotation.eulerAngles;
        targetEuler.z = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, matchPlaneRotationSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, matchPlaneRotationSpeed * Time.deltaTime);
    }

    public void SetForwardSpeed(float speed) { forwardSpeed = speed; }
    public void SetRotatingSpeed(float speed) { rotatingSpeed = speed; }

    public void Move(float horizontalInput)
    {
        GameObject[] tunnels = GameObject.FindGameObjectsWithTag("Center");
        if (tunnels != null && tunnels.Length > 0)
        {
            float rotationBoost = 1f;

#if UNITY_ANDROID && !UNITY_EDITOR
    rotationBoost = mobileRotationMultiplier;
#endif

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

    public void OnMove(InputAction.CallbackContext context)
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        moveInput = context.ReadValue<Vector2>();
#endif
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            if (audioSource != null)
                audioSource.Play();
            // Make the chaser jump too
            if (chaserScript != null)
            {
                chaserScript.Jump();
            }
        }
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
    }

    void OnDisable()
    {
        tiltAction.Disable();
    }
}
