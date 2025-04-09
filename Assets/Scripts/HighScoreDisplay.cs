using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

public class HighScoreDisplay : MonoBehaviour
{
    [Header("Leaderboard UI Elements")]
    [SerializeField] private TMP_Text[] rankTexts;
    [SerializeField] private TMP_Text[] usernameTexts;
    [SerializeField] private TMP_Text[] scoreTexts;

    [Header("Current Session UI")]
    [SerializeField] private TMP_Text currentDebtPaidText;
    [SerializeField] private TMP_Text currentMoneyMadeText;
    [SerializeField] private TMP_Text currentPlayTimeText;
    [SerializeField] private TMP_Text currentCompositeScoreText;
    [SerializeField] private TMP_Text currentUsernameText;

    private async void Start()
    {
        await LoadLeaderboardAsync();
        UpdateCurrentSessionUI();
    }

    private async Task LoadLeaderboardAsync()
    {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync("GambleRun", new GetScoresOptions { Limit = 5 });
            entries = scoresResponse.Results;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to fetch leaderboard scores: " + e.Message);
        }

        DisplayLeaderboard(entries);
    }

    private void DisplayLeaderboard(List<LeaderboardEntry> entries)
    {
        string currentUsername = PlayerPrefs.GetString("username", "Guest");

        for (int i = 0; i < rankTexts.Length; i++)
        {
            if (i < entries.Count)
            {
                var entry = entries[i];
                string usernameRaw = entry.PlayerName ?? "Anonymous";
                string username = usernameRaw.Contains("#") ? usernameRaw.Split('#')[0] : usernameRaw;

                long score = (long)entry.Score;

                rankTexts[i].text = $"{i + 1}.";
                usernameTexts[i].text = username;
                scoreTexts[i].text = $"{score}";

                if (username.Equals(currentUsername, StringComparison.OrdinalIgnoreCase))
                {
                    rankTexts[i].color = Color.green;
                    usernameTexts[i].color = Color.green;
                    scoreTexts[i].color = Color.green;
                }
                else if (i == 0)
                {
                    Color gold = new Color(1f, 0.84f, 0f);
                    rankTexts[i].color = gold;
                    usernameTexts[i].color = gold;
                    scoreTexts[i].color = gold;
                }
                else
                {
                    rankTexts[i].color = Color.red;
                    usernameTexts[i].color = Color.red;
                    scoreTexts[i].color = Color.red;
                }
            }
            else
            {
                rankTexts[i].text = $"{i + 1}.";
                usernameTexts[i].text = "N/A";
                scoreTexts[i].text = "0";

                rankTexts[i].color = Color.white;
                usernameTexts[i].color = Color.white;
                scoreTexts[i].color = Color.white;
            }
        }
    }

    private void UpdateCurrentSessionUI()
    {
        float sessionDebtPaid = MoneyManager.Instance != null ? MoneyManager.Instance.GetSessionDebtPaid() : 0f;
        float sessionMoneyMade = MoneyManager.Instance != null ? MoneyManager.Instance.GetSessionMoneyObtained() : 0f;
        float sessionPlayTime = PlayerPrefs.GetFloat("PlayTime", 0f);
        float compositeScoreFloat = Mathf.Ceil(
    (sessionMoneyMade + sessionDebtPaid) * (0.5f * sessionPlayTime)
);
        int compositeScore = Mathf.RoundToInt(compositeScoreFloat);
        string username = PlayerPrefs.GetString("username", "Guest");

        if (currentDebtPaidText != null)
            currentDebtPaidText.text = $"€{sessionDebtPaid:F2}";
        if (currentMoneyMadeText != null)
            currentMoneyMadeText.text = $"€{sessionMoneyMade:F2}";
        if (currentPlayTimeText != null)
            currentPlayTimeText.text = $"{sessionPlayTime:F2} sec";
        if (currentCompositeScoreText != null)
            currentCompositeScoreText.text = $"{compositeScore}";
        if (currentUsernameText != null)
            currentUsernameText.text = username;
    }
}
