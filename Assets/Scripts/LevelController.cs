using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Analytics;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    // Tracks total playtime in seconds for the current session.
    private float sessionPlayTime = 0f;
    private bool gameOverHandled = false;
    // Count of how many times the player has replayed.
    private int replayCount = 0;
    // This event is raised when the game is over.
    public static event Action OnGameOverEvent;
    // Whether DDS is enabled.
    public bool ddsEnabled = true;

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
    private void OnEnable()
    {
        OutsideMapCheck.OnOutsideMapGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        OutsideMapCheck.OnOutsideMapGameOver -= HandleGameOver;
    }

    private void Update()
    {
        // Accumulate playtime.
        sessionPlayTime += Time.deltaTime;
    }

    public async void HandleGameOver()
    {
        if (gameOverHandled) return;
        gameOverHandled = true;
        OnGameOverEvent?.Invoke();

        if (InfiniteRunnerMovement.Instance != null)
            InfiniteRunnerMovement.Instance.SetGameOver(true);

        Debug.Log("Game Over! Debt limit reached.");
        Debug.Log("Total playtime: " + sessionPlayTime.ToString("F2") + " seconds");

        PlayerPrefs.SetFloat("PlayTime", sessionPlayTime);

        float moneyObtained = MoneyManager.Instance.GetSessionMoneyObtained();
        float debtPaid = MoneyManager.Instance.GetSessionDebtPaid();
        float currentMoney = MoneyManager.Instance.GetMoney();
        float moneyLost = moneyObtained - currentMoney;
        // Retrieve the player's chosen username.
        string username = PlayerPrefs.GetString("username", "Guest");

        // Optionally set the Analytics Service User ID to the username.
        // AnalyticsService.Instance.UserId = username;

        // Submit composite score to the leaderboard.
        await LeaderboardManager.Instance.SubmitScoreAsync(moneyObtained, debtPaid, sessionPlayTime);

        // Prepare and record the session analytics event.
        SessionEndEvent sessionEvent = new SessionEndEvent();
        sessionEvent.username = username;
        sessionEvent.total_sessiontime = sessionPlayTime;
        sessionEvent.replay_count = replayCount;
        sessionEvent.money_made = moneyObtained;
        sessionEvent.debt_paid = debtPaid;
        sessionEvent.money_lost = moneyLost;
        sessionEvent.dds_enabled = ddsEnabled;
        AnalyticsService.Instance.RecordEvent(sessionEvent);

        Dictionary<string, object> ddsData = DDSManager.Instance.GetCurrentDDSAnalytics();
        AnalyticsService.Instance.RecordEvent(new DDSDifficultyEvent
        {
            avgSlotProbability = (float)ddsData["avgSlotProbability"],
            difficultyLevel = (string)ddsData["difficultyLevel"],
            debtRatio = (float)ddsData["debtRatio"],
            moneyRatio = (float)ddsData["moneyRatio"]
        });

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddSessionStats(moneyObtained, debtPaid, sessionPlayTime);

        MoneyManager.Instance.SaveData();
        PlayerPrefs.Save();
        SceneManager.LoadScene("EndScore");
    }

    public void ResetGame()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.ResetSession();
        if (InfiniteRunnerMovement.Instance != null)
            InfiniteRunnerMovement.Instance.SetGameOver(false);
        sessionPlayTime = 0f;
        gameOverHandled = false;
        replayCount++;
        SceneManager.LoadScene("WhiteBOX");
    }
}
