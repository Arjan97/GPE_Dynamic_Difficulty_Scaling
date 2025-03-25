using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    [SerializeField] private float defaultAmplitude = 1f;
    [SerializeField] private float defaultFrequency = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
        else
        {
            Debug.LogWarning("CameraShake: No CinemachineVirtualCamera found on this GameObject.");
        }
    }

    /// <summary>
    /// Shakes the camera for a given duration, amplitude, and frequency.
    /// </summary>
    public void ShakeScreen(float duration, float amplitude, float frequency)
    {
        if (noise == null)
        {
            Debug.LogWarning("CameraShake: No noise component found on the virtual camera.");
            return;
        }
        StartCoroutine(ShakeRoutine(duration, amplitude, frequency));
    }

    private IEnumerator ShakeRoutine(float duration, float amplitude, float frequency)
    {
        // Store the original settings.
        float originalAmplitude = noise.AmplitudeGain;
        float originalFrequency = noise.FrequencyGain;

        // Set new shake parameters.
        noise.AmplitudeGain = amplitude;
        noise.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        // Restore original settings.
        noise.AmplitudeGain = originalAmplitude;
        noise.FrequencyGain = originalFrequency;
    }
}
