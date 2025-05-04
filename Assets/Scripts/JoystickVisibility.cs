using UnityEngine;

public class JoystickVisibility : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(SettingsManager.Instance.JoystickEnabled);

        SettingsManager.Instance.OnJoystickSettingChanged += HandleJoystickSettingChanged;
    }

    void OnDestroy()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnJoystickSettingChanged -= HandleJoystickSettingChanged;
    }

    void HandleJoystickSettingChanged(bool enabled)
    {
        gameObject.SetActive(enabled);
    }
}
