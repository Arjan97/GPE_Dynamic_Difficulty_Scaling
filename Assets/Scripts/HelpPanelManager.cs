using UnityEngine;

public class HelpPanelManager : MonoBehaviour
{
    [Header("Help Panels")]
    [SerializeField] private GameObject mobileHelpPanel;
    [SerializeField] private GameObject desktopHelpPanel;
    [SerializeField] private GameObject toDisableBackdrop;

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
        if (toDisableBackdrop != null)
            toDisableBackdrop.SetActive(false);
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called from the Close Button on the panel.
    /// Hides the currently active help panel.
    /// </summary>
    public void HideHelpPanel()
    {
        this.gameObject.SetActive(true);
        if (currentPanel != null)
            currentPanel.SetActive(false);
        if (toDisableBackdrop != null)
            toDisableBackdrop.SetActive(true);
    }
}
