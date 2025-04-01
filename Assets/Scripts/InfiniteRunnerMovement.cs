using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class InfiniteRunnerMovement : MonoBehaviour
{
    public static InfiniteRunnerMovement Instance { get; private set; }
    [Header("Camera Rotator Clamp")]
    [Tooltip("Minimum local X rotation for the camRotator.")]
    public float clampMin = -160f;
    [Tooltip("Maximum local X rotation for the camRotator.")]
    public float clampMax = 160f;
    [Header("Movement Settings")]
    public float forwardSpeed = 5f;    // Constant forward movement (along X-axis)
    public float rotatingSpeed = 5f; // Input-based rotation speed for the tunnel
    public float matchPlaneRotationSpeed = 10f;  // Speed at which player aligns to new ground

    [Header("Jump Settings")]
    public float jumpForce = 7f;       // Force applied upward when jumping
    public Transform groundCheck;      // Transform with SphereCollider for ground detection
    public LayerMask groundLayer;      // Layer(s) considered ground

    private Rigidbody rb;
    private Vector2 moveInput;

    // Public property to expose the horizontal input (from OnMove).
    public float HorizontalInput => moveInput.x;

    private bool isGrounded;
    private SphereCollider groundChecker;
    private Transform lastGroundHit;
    private PlayerAnimatorController animController;

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        groundChecker = groundCheck.GetComponent<SphereCollider>();
    }

    private void Start()
    {
        animController = GameObject.FindGameObjectWithTag("PlayerAnim").GetComponent<PlayerAnimatorController>();
    }

    void Update()
    {
        CheckGround();
    }

    void FixedUpdate()
    {
        MoveTunnel();
    }

    void CheckGround()
    {
        Collider[] colliders = new Collider[3]; // small array to prevent GC allocations
        int hitCount = Physics.OverlapSphereNonAlloc(groundCheck.position, groundChecker.radius, colliders, groundLayer);
        isGrounded = false;

        animController?.SetGroundedState(false);

        for (int i = 0; i < hitCount; i++)
        {
            if (colliders[i] != null && colliders[i].gameObject != gameObject) // Ignore self
            {
                isGrounded = true;
                lastGroundHit = colliders[i].transform;
                AlignToGround(colliders[i].transform.up);
                animController?.SetGroundedState(true);
                break;
            }
        }
    }

    // Aligns the player's up vector with the ground normal,
    // while smoothly matching the platform's roll if needed.
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

    void MoveTunnel()
    {
        // In this method, the tunnel rotates based on horizontal input.
        GameObject[] tunnels = GameObject.FindGameObjectsWithTag("Center");
        if (tunnels != null && tunnels.Length > 0)
        {
            float rotationAmount = -moveInput.x * rotatingSpeed * Time.deltaTime;
            foreach (GameObject tunnel in tunnels)
            {
                tunnel.transform.Rotate(rotationAmount, 0, 0);
                tunnel.transform.Translate(Vector3.left * forwardSpeed * Time.deltaTime, Space.World);
            }
        }
        GameObject camRotator = GameObject.FindGameObjectWithTag("Rotate");
        if (camRotator != null)
        {
            camRotator.transform.Rotate(0, 0, -moveInput.x * rotatingSpeed * Time.deltaTime);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Perform jump if the player is grounded.
            if (isGrounded)
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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
}
