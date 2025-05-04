using UnityEngine;
using UnityEngine.UI;

public class SoundToggleVisual : MonoBehaviour
{
    [SerializeField] Image targetImage = null;
    [SerializeField] Sprite soundOnSprite = null;
    [SerializeField] Sprite soundOffSprite = null;
    [SerializeField] Button toggleButton = null;

    void Start()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(() => SettingsManager.Instance.ToggleSound());

        UpdateVisual(SettingsManager.Instance.SoundEnabled);
        SettingsManager.Instance.OnSoundSettingChanged += UpdateVisual;
    }

    void OnDestroy()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSoundSettingChanged -= UpdateVisual;
    }

    void UpdateVisual(bool enabled)
    {
        if (targetImage != null)
            targetImage.sprite = enabled ? soundOnSprite : soundOffSprite;
    }
}
