using UnityEngine;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    const string JoystickPrefKey = "JoystickEnabled";
    const string SoundPrefKey = "SoundEnabled";

    bool _joystickEnabled = false;
    bool _soundEnabled = true;

    public bool JoystickEnabled => _joystickEnabled;
    public bool SoundEnabled => _soundEnabled;

    public event Action<bool> OnJoystickSettingChanged;
    public event Action<bool> OnSoundSettingChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // For non-mobile builds, force the joystick setting off.
        if (Application.isMobilePlatform)
        {
            LoadSettings();
        }
        else
        {
            _joystickEnabled = false; 
        }
        ApplySoundSetting();
    }

    void LoadSettings()
    {
        _joystickEnabled = PlayerPrefs.GetInt(JoystickPrefKey, 1) == 1;
        _soundEnabled = PlayerPrefs.GetInt(SoundPrefKey, 1) == 1;
    }

    public void SetJoystickEnabled(bool enabled)
    {
        if (_joystickEnabled == enabled)
            return;

        _joystickEnabled = enabled;
        PlayerPrefs.SetInt(JoystickPrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

        OnJoystickSettingChanged?.Invoke(enabled);
    }

    public void ToggleJoystick()
    {
        SetJoystickEnabled(!_joystickEnabled);
    }

    public void SetSoundEnabled(bool enabled)
    {
        if (_soundEnabled == enabled)
            return;

        _soundEnabled = enabled;
        PlayerPrefs.SetInt(SoundPrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplySoundSetting();

        OnSoundSettingChanged?.Invoke(enabled);
    }


    public void ToggleSound()
    {
        SetSoundEnabled(!_soundEnabled);
    }

    void ApplySoundSetting()
    {
        AudioListener.volume = _soundEnabled ? 1f : 0f;
    }
}
