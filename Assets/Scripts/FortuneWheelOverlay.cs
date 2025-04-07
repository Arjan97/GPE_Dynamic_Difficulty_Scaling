using System.Collections;
using UnityEngine;
using JSG.FortuneSpinWheel;  // Ensure you have the proper namespace for FortuneSpinWheel
using UnityEngine.Audio;

public class FortuneWheelOverlay : MonoBehaviour
{
    [Tooltip("Reference to the Fortune Spin Wheel overlay GameObject.")]
    [SerializeField] private GameObject fortuneWheelOverlayObject;

    [Tooltip("Delay (in seconds) after the reward panel is shown before hiding the overlay.")]
    [SerializeField] private float closeDelay = 1f;

    private FortuneSpinWheel fortuneSpinWheel;
    private bool wheelShown = false;
    private AudioSource audioSource;
    private void Awake()
    {
        // If no separate overlay object is assigned, assume the script is on it.
        if (fortuneWheelOverlayObject == null)
            fortuneWheelOverlayObject = gameObject;

        // Get the FortuneSpinWheel component from the overlay object.
        fortuneSpinWheel = fortuneWheelOverlayObject.GetComponent<FortuneSpinWheel>();

        // Ensure the overlay is disabled by default.
        fortuneWheelOverlayObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Enables the Fortune Spin Wheel overlay and starts a coroutine to close it after the reward panel is shown.
    /// </summary>
    public void ShowWheel()
    {
        if (wheelShown)
        {
            return;
        }
        // Enable the overlay.
        fortuneWheelOverlayObject.SetActive(true);
        // Play overlay open sound.
        if (audioSource != null)
        {
            audioSource.Play();
        }
        wheelShown = true;
        // Optionally reset the wheel if needed.
        if (fortuneSpinWheel != null)
        {
            fortuneSpinWheel.Reset();
            
        }

        // Start waiting for the reward panel to appear, then close the overlay.
        StartCoroutine(WaitForRewardPanelAndClose());
    }

    /// <summary>
    /// Waits until the reward panel in the FortuneSpinWheel is active, then waits for a delay before hiding the overlay.
    /// </summary>
    private IEnumerator WaitForRewardPanelAndClose()
    {
        // Wait until the reward panel is active.
        while (fortuneSpinWheel != null && !fortuneSpinWheel.m_RewardPanel.activeSelf)
        {
            yield return null;
        }

        // Once active, wait for the specified delay.
        yield return new WaitForSeconds(closeDelay);

        // Hide the overlay.
        fortuneWheelOverlayObject.SetActive(false);
        wheelShown = false;
    }
}
