using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    [Header("Debt Paid (Top 5)")]
    [SerializeField] private TMP_Text[] debtPaidTexts;

    [Header("Money Made (Top 5)")]
    [SerializeField] private TMP_Text[] moneyMadeTexts;

    [Header("Play Time (Top 5)")]
    [SerializeField] private TMP_Text[] playTimeTexts;

    // For current session:
    [Header("Current Session UI")]
    [SerializeField] private TMP_Text currentDebtPaidText;
    [SerializeField] private TMP_Text currentMoneyMadeText;
    [SerializeField] private TMP_Text currentPlayTimeText;

    private void Start()
    {
        // Load top-5
        UpdateHighScoreUI("DebtPaid", debtPaidTexts, "€{0:F2}");
        UpdateHighScoreUI("MoneyMade", moneyMadeTexts, "€{0:F2}");
        UpdateHighScoreUI("PlayTime", playTimeTexts, "{0:F2} sec");

        // Display current session
        float sessionDebtPaid = MoneyManager.Instance != null ? MoneyManager.Instance.GetSessionDebtPaid() : 0f;
        float sessionMoneyMade = MoneyManager.Instance != null ? MoneyManager.Instance.GetSessionMoneyObtained() : 0f;
        float sessionPlayTime = PlayerPrefs.GetFloat("PlayTime", 0f);

        if (currentDebtPaidText != null)
            currentDebtPaidText.text = $"€{sessionDebtPaid:F2}";

        if (currentMoneyMadeText != null)
            currentMoneyMadeText.text = $"€{sessionMoneyMade:F2}";

        if (currentPlayTimeText != null)
            currentPlayTimeText.text = $"{sessionPlayTime:F2} sec";
    }

    private void UpdateHighScoreUI(string keyPrefix, TMP_Text[] textFields, string format)
    {
        for (int i = 0; i < textFields.Length; i++)
        {
            string key = keyPrefix + "_" + (i + 1);
            float score = PlayerPrefs.GetFloat(key, 0f);
            textFields[i].text = $"{i + 1}. " + string.Format(format, score);
        }
    }
}
