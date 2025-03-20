using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class InfiniteRunnerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 5f;       // Constant forward movement (Z-axis)
    public float horizontalSpeed = 5f;    // Left/right movement speed
    public float rotationSpeed = 10f;     // Speed at which player aligns to new ground

    [Header("Jump Settings")]
    public float jumpForce = 7f;          // Force applied upward when jumping
    public Transform groundCheck;         // Transform with SphereCollider for ground detection
    public LayerMask groundLayer;         // Layer(s) considered ground

    private Rigidbody rb;
    private Vector2 moveInput;
    private Transform lastGroundHit;      // Last ground the player touched
    private bool isGrounded;
    private SphereCollider groundChecker;

    private GameObject hexTunnel;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        groundChecker = groundCheck.GetComponent<SphereCollider>();
    }

    void Update()
    {
        CheckGround();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void CheckGround()
    {
        Collider[] colliders = new Collider[3]; // Small array to prevent GC allocations
        int hitCount = Physics.OverlapSphereNonAlloc(groundCheck.position, groundChecker.radius, colliders, groundLayer);
        isGrounded = false;
        for (int i = 0; i < hitCount; i++)
        {
            if (colliders[i] != null && colliders[i].gameObject != gameObject) // Ignore self
            {
                isGrounded = true;
                lastGroundHit = colliders[i].transform;
                AlignToGround(colliders[i].transform.up);
                break;
            }
        }
    }

    // Aligns the player's up vector with the ground normal while smoothly matching the platform's roll (Z rotation)
    void AlignToGround(Vector3 groundNormal)
    {
        // Compute a rotation that aligns the player's up to the ground normal.
        Quaternion upAlignedRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;

        // Determine the target Z rotation (roll) from the ground platform.
        // If lastGroundHit is valid, use its Z Euler angle; otherwise, maintain current roll.
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

    void MovePlayer()
    {
        // Move player forward continuously.
        Vector3 forwardMovement = transform.forward * forwardSpeed;
        // Preserve current vertical velocity.
        Vector3 currentVelocity = rb.linearVelocity;
        // Set X to 0 (if no lateral movement is applied to the player) and update forward (Z) velocity.
        currentVelocity = new Vector3(0, currentVelocity.y, forwardMovement.z);
        rb.linearVelocity = currentVelocity;

        // Rotate tunnel objects based on horizontal input.
        GameObject[] tunnels = GameObject.FindGameObjectsWithTag("Center");
        if (tunnels != null && tunnels.Length > 0)
        {
            float rotationAmount = moveInput.x * horizontalSpeed * Time.deltaTime;
            foreach (GameObject tunnel in tunnels)
            {
                // Rotate the tunnel around its local Z-axis.
                tunnel.transform.Rotate(0, 0, -rotationAmount);
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

    // Draws a wire sphere in the Scene view to visualize the ground check area.
    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            // Attempt to get the SphereCollider from the groundCheck if it's not already set.
            SphereCollider sc = groundChecker != null ? groundChecker : groundCheck.GetComponent<SphereCollider>();
            if (sc != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, sc.radius);
            }
        }
    }
}
