using System.Collections.Generic;
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
        // Retrieve top sessions stored under one PlayerPrefs key
        List<SessionStats> topSessions = ScoreManager.Instance.GetSessionStats();

        // Update the Top Sessions UI for Debt Paid
        for (int i = 0; i < debtPaidTexts.Length; i++)
        {
            if (i < topSessions.Count)
                debtPaidTexts[i].text = $"{i + 1}. €{topSessions[i].debtPaid:F2}";
            else
                debtPaidTexts[i].text = $"{i + 1}. €0.00";
        }

        // Update the Top Sessions UI for Money Made
        for (int i = 0; i < moneyMadeTexts.Length; i++)
        {
            if (i < topSessions.Count)
                moneyMadeTexts[i].text = $"{i + 1}. €{topSessions[i].moneyObtained:F2}";
            else
                moneyMadeTexts[i].text = $"{i + 1}. €0.00";
        }

        // Update the Top Sessions UI for Play Time
        for (int i = 0; i < playTimeTexts.Length; i++)
        {
            if (i < topSessions.Count)
                playTimeTexts[i].text = $"{i + 1}. {topSessions[i].playTime:F2} sec";
            else
                playTimeTexts[i].text = $"{i + 1}. 0.00 sec";
        }

        // Display the current session stats separately
        float sessionDebtPaid = MoneyManager.Instance != null ? MoneyManager.Instance.GetSessionDebtPaid() : 0f;
        float sessionMoneyMade = MoneyManager.Instance != null ? MoneyManager.Instance.GetSessionMoneyObtained() : 0f;
        // Retrieve session play time; adjust this source if you track it elsewhere
        float sessionPlayTime = PlayerPrefs.GetFloat("PlayTime", 0f);

        if (currentDebtPaidText != null)
            currentDebtPaidText.text = $"€{sessionDebtPaid:F2}";
        if (currentMoneyMadeText != null)
            currentMoneyMadeText.text = $"€{sessionMoneyMade:F2}";
        if (currentPlayTimeText != null)
            currentPlayTimeText.text = $"{sessionPlayTime:F2} sec";
    }
}
