using UnityEngine;
using UnityEngine.UI;

public class JoystickToggleInitializer : MonoBehaviour
{
    [SerializeField] Image targetImage = null;
    [SerializeField] Button toggleButton = null;
    [SerializeField] Sprite enabledSprite = null;
    [SerializeField] Sprite disabledSprite = null;
    void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(() => SettingsManager.Instance.ToggleJoystick());

        UpdateVisual(SettingsManager.Instance.JoystickEnabled);
        SettingsManager.Instance.OnJoystickSettingChanged += UpdateVisual;
    }
    void OnDestroy()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnJoystickSettingChanged -= UpdateVisual;
    }
    void UpdateVisual(bool enabled)
    {
        if (targetImage != null)
            targetImage.sprite = enabled ? enabledSprite : disabledSprite;
    }
}
