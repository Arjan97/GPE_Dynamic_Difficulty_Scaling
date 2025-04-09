using UnityEngine;

public class HelpPanelManager : MonoBehaviour
{
    [Header("Help Panels")]
    [SerializeField] private GameObject mobileHelpPanel;
    [SerializeField] private GameObject desktopHelpPanel;

    private GameObject currentPanel;

    /// <summary>
    /// Called from the Help Button.
    /// Shows the platform-specific help panel.
    /// </summary>
    public void ShowHelpPanel()
    {
#if UNITY_ANDROID && !UNITY_EDITOR || UNITY_IOS && !UNITY_EDITOR
        currentPanel = mobileHelpPanel;
#else
        currentPanel = desktopHelpPanel;
#endif
        if (currentPanel != null)
            currentPanel.SetActive(true);
    }

    /// <summary>
    /// Called from the Close Button on the panel.
    /// Hides the currently active help panel.
    /// </summary>
    public void HideHelpPanel()
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);
    }
}
