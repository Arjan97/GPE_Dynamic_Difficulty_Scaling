using UnityEngine;
using Michsky.UI.Heat; // For the ProgressBar type

public class SceneSetup : MonoBehaviour
{
    [Header("ProgressBar References (Optional)")]
    [Tooltip("Assign the Money ProgressBar here or leave empty to auto-find by name.")]
    [SerializeField] private ProgressBar moneyProgressBar;
    [Tooltip("Assign the Debt ProgressBar here or leave empty to auto-find by name.")]
    [SerializeField] private ProgressBar debtProgressBar;

    private void Start()
    {
        // Ensure MoneyManager exists
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager instance not found!");
            return;
        }

        // Clear old listeners to avoid duplicates if the scene reloads.
        MoneyManager.Instance.OnMoneyChanged.RemoveAllListeners();
        MoneyManager.Instance.OnDebtChanged.RemoveAllListeners();

        // If the progress bars weren't assigned in the Inspector, try to find them by name.
        if (moneyProgressBar == null)
        {
            GameObject moneyBarGO = GameObject.Find("MoneyBar");
            if (moneyBarGO != null)
                moneyProgressBar = moneyBarGO.GetComponent<ProgressBar>();
        }

        if (debtProgressBar == null)
        {
            GameObject debtBarGO = GameObject.Find("DebtBar");
            if (debtBarGO != null)
                debtProgressBar = debtBarGO.GetComponent<ProgressBar>();
        }

        // Subscribe the ProgressBar.SetValue method to MoneyManager events.
        if (moneyProgressBar != null)
        {
            MoneyManager.Instance.OnMoneyChanged.AddListener(moneyProgressBar.SetValue);
            // Immediately update the UI to reflect the current money value.
            moneyProgressBar.SetValue(MoneyManager.Instance.GetMoney());
        }
        else
        {
            Debug.LogWarning("MoneyProgressBar not found in the scene.");
        }

        if (debtProgressBar != null)
        {
            MoneyManager.Instance.OnDebtChanged.AddListener(debtProgressBar.SetValue);
            // Immediately update the UI to reflect the current debt value.
            debtProgressBar.SetValue(MoneyManager.Instance.GetDebt());
        }
        else
        {
            Debug.LogWarning("DebtProgressBar not found in the scene.");
        }
    }
}
