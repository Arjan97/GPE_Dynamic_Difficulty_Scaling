using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;

    // Cache the Animator component on awake.
    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the player.");
        }
    }

    // Public method to set the 'isGrounded' parameter.
    public void SetGroundedState(bool isGrounded)
    {
        if (animator != null)
        {
            animator.SetBool("isGrounded", isGrounded);
        }
    }
}
