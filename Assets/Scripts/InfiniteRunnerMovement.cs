using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class InfiniteRunnerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 5f;    // Constant forward movement (now along X-axis)
    public float horizontalSpeed = 5f; // Input-based rotation speed for the tunnel
    public float rotationSpeed = 10f;  // Speed at which player aligns to new ground

    [Header("Jump Settings")]
    public float jumpForce = 7f;       // Force applied upward when jumping
    public Transform groundCheck;      // Transform with SphereCollider for ground detection
    public LayerMask groundLayer;      // Layer(s) considered ground

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private SphereCollider groundChecker;
    private Transform lastGroundHit;
    private PlayerAnimatorController animController;

    void Awake()
    {
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
        // Compute a rotation that aligns the player's up to the ground normal.
        Quaternion upAlignedRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;

        // Determine the target "roll" from the ground platform�s local rotation, if needed.
        float targetZRotation = lastGroundHit != null ? lastGroundHit.eulerAngles.z : transform.eulerAngles.z;

        // Get the Euler angles from the up-aligned rotation.
        Vector3 targetEuler = upAlignedRotation.eulerAngles;
        // Smoothly interpolate the current roll towards the target roll.
        targetEuler.z = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, rotationSpeed * Time.deltaTime);

        // Build the final target rotation.
        Quaternion targetRotation = Quaternion.Euler(targetEuler);
        // Smoothly interpolate from the current rotation to the target rotation.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void MoveTunnel()
    {
        /* Instead of moving player forward ill move the tunnel towards the player 
        // Move the player forward along X (transform.right).
        Vector3 forwardMovement = transform.right * forwardSpeed;

        // Preserve vertical velocity, zero out Z, and apply forward X velocity.
        Vector3 currentVelocity = rb.linearVelocity;
        currentVelocity.x = forwardMovement.x;  // X is forward
        currentVelocity.z = 0f;                 // No movement on Z
        rb.linearVelocity = currentVelocity;
        */

        // Rotate tunnel objects based on horizontal input.
        GameObject[] tunnels = GameObject.FindGameObjectsWithTag("Center");
        if (tunnels != null && tunnels.Length > 0)
        {
            // Horizontal input is moveInput.x; 
            // negative sign can be adjusted if rotation is reversed from what you want.
            float rotationAmount = -moveInput.x * horizontalSpeed * Time.deltaTime;

            foreach (GameObject tunnel in tunnels)
            {
                // Rotate the tunnel around its local X-axis 
                tunnel.transform.Rotate(rotationAmount, 0, 0);
                // Move tunnel towards player along its local X-axis
                tunnel.transform.Translate(Vector3.left * forwardSpeed * Time.deltaTime, Space.World);
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
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
