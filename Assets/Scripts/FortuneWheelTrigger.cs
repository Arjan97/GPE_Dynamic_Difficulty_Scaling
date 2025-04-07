using System.Collections;
using UnityEngine;
using JSG.FortuneSpinWheel;

public class FortuneWheelTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check for the player tag and that this trigger hasn't already been activated.
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            // Find the FortuneSpinWheel object in the scene.
            FortuneWheelOverlay wheel = FindFirstObjectByType<FortuneWheelOverlay>();
            if (wheel != null)
            {
                // Toggle the Fortune Wheel overlay on.
                wheel.ShowWheel();
            }
        }
    }
}
