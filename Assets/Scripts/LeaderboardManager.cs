using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    private const string leaderboardId = "GambleRun";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Submits the composite score to the GambleRun leaderboard.
    /// Composite score is calculated as:
    /// compositeScore = Ceil((moneyMade + debtPaid) * (0.5f * playTime))
    /// </summary>
    public async Task SubmitScoreAsync(float moneyMade, float debtPaid, float playTime)
    {
        float compositeScoreFloat = Mathf.Ceil((moneyMade + debtPaid) * (0.5f * playTime));
        int compositeScore = Mathf.RoundToInt(compositeScoreFloat);
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, compositeScore);
            Debug.Log($"LeaderboardManager: Submitted composite score: {compositeScore}");
        }
        catch (Exception e)
        {
            Debug.LogError("LeaderboardManager: Failed to submit score: " + e.Message);
        }
    }
    public async Task<int> GetMyBestScoreAsync()
    {
        try
        {
            var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
            return (int)scoreResponse.Score;
        }
        catch (Exception e)
        {
            Debug.LogError("LeaderboardManager: Failed to fetch player's best score: " + e.Message);
            return 0;
        }
    }

    /// <summary>
    /// Fetches the top leaderboard entries.
    /// </summary>
    public async Task<List<LeaderboardEntry>> FetchTopScoresAsync(int limit = 5)
    {
        try
        {
            GetScoresOptions options = new GetScoresOptions { Limit = limit };
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);
            return scoresResponse.Results;
        }
        catch (Exception e)
        {
            Debug.LogError("LeaderboardManager: Failed to fetch leaderboard scores: " + e.Message);
            return new List<LeaderboardEntry>();
        }
    }
}
