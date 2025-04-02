using UnityEngine;
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    [Header("Total Debt Paid Scores UI")]
    [Tooltip("Text fields for the top 5 'Total Debt Paid' scores (rank 1 at index 0).")]
    [SerializeField] private TMP_Text[] debtPaidTexts; // Should have 5 elements

    [Header("Total Money Obtained Scores UI")]
    [Tooltip("Text fields for the top 5 'Total Money Obtained' scores.")]
    [SerializeField] private TMP_Text[] moneyObtainedTexts; // Should have 5 elements

    [Header("Playtime Scores UI")]
    [Tooltip("Text fields for the top 5 'Playtime' scores.")]
    [SerializeField] private TMP_Text[] playTimeTexts; // Should have 5 elements

    [Header("Current Session Scores UI")]
    [Tooltip("Text field to display the current total debt paid.")]
    [SerializeField] private TMP_Text currentDebtPaidText;
    [Tooltip("Text field to display the current total money obtained.")]
    [SerializeField] private TMP_Text currentMoneyObtainedText;
    [Tooltip("Text field to display the current playtime.")]
    [SerializeField] private TMP_Text currentPlayTimeText;
    private const int maxScores = 5;

    private void Start()
    {
        // For each category, retrieve scores from PlayerPrefs.
        // Here, we assume that each score is stored with keys like "DebtPaid_1", "DebtPaid_2", etc.
        UpdateHighScoreUI("DebtPaid", debtPaidTexts, "€{0:F2}");
        UpdateHighScoreUI("MoneyMade", moneyObtainedTexts, "€{0:F2}");
        UpdateHighScoreUI("PlayTime", playTimeTexts, "{0:F2} sec");

        // Update the current session scores.
        float currentDebtPaid = PlayerPrefs.GetFloat("TotalDebtPaid", 0f);
        float currentMoneyObtained = PlayerPrefs.GetFloat("TotalMoneyObtained", 0f);
        float playTime = PlayerPrefs.GetFloat("PlayTime", 0f);

        if (currentDebtPaidText != null)
            currentDebtPaidText.text = "€" + currentDebtPaid.ToString("F2");

        if (currentMoneyObtainedText != null)
            currentMoneyObtainedText.text = "€" + currentMoneyObtained.ToString("F2");

        if (currentPlayTimeText != null)
            currentPlayTimeText.text = playTime.ToString("F2") + " sec";
    }

    /// <summary>
    /// Retrieves the top scores from PlayerPrefs for the given key prefix and updates the text fields.
    /// </summary>
    /// <param name="keyPrefix">The key prefix (e.g., "TotalDebtPaid").</param>
    /// <param name="textFields">Array of TMP_Text UI elements (should be 5 elements).</param>
    /// <param name="formatString">Format string (e.g., "€{0:F2}" or "{0:F2} sec").</param>
    private void UpdateHighScoreUI(string keyPrefix, TMP_Text[] textFields, string formatString)
    {
        for (int i = 0; i < textFields.Length; i++)
        {
            string key = keyPrefix + "_" + (i + 1);
            float score = PlayerPrefs.GetFloat(key, 0f);
            textFields[i].text = $"{i + 1}. " + string.Format(formatString, score);
        }
    }
}
