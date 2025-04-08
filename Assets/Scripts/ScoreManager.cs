using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SessionStats
{
    public string sessionID;
    public string username;        // The player's chosen username.
    public float moneyObtained;
    public float debtPaid;
    public float playTime;         // in seconds
    public int compositeScore;     // Combined score

    public SessionStats(string sessionID, string username, float moneyObtained, float debtPaid, float playTime, int compositeScore)
    {
        this.sessionID = sessionID;
        this.username = username;
        this.moneyObtained = moneyObtained;
        this.debtPaid = debtPaid;
        this.playTime = playTime;
        this.compositeScore = compositeScore;
    }
}

[Serializable]
public class SessionStatsList
{
    public List<SessionStats> sessions;
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    // We now allow at most 10 sessions.
    private const int maxSessions = 10;
    private const string sessionStatsKey = "SessionStats";

    // List of all session records stored locally.
    private List<SessionStats> allSessions = new List<SessionStats>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSessionStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (CloudSaveManager.Instance != null)
            {
                await CloudSaveManager.Instance.SignUpAnonymouslyAsync();
            }
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("Unity Services Initialized.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Unity Services: " + e.Message);
        }
    }
    /// <summary>
    /// Call this method at the end of a session to add its stats.
    /// If fewer than 10 sessions are stored, the new session is added.
    /// Once 10 sessions exist, the new session replaces the lowest composite score if it is higher.
    /// </summary>
    public async void AddSessionStats(float moneyObtained, float debtPaid, float playTime)
    {
        // Calculate composite score as: compositeScore = moneyObtained + debtPaid + (0.5 * playTime)
        int compositeScore = Mathf.RoundToInt(moneyObtained + debtPaid + (0.5f * playTime));
        string sessionID = Guid.NewGuid().ToString();

        // Retrieve username that was previously registered.
        string username = PlayerPrefs.GetString("username", "Guest");

        SessionStats newSession = new SessionStats(sessionID, username, moneyObtained, debtPaid, playTime, compositeScore);
        if (allSessions.Count < maxSessions)
        {
            allSessions.Add(newSession);
        }
        else
        {
            SessionStats lowest = allSessions.OrderBy(s => s.compositeScore).First();
            if (newSession.compositeScore > lowest.compositeScore)
            {
                allSessions.Remove(lowest);
                allSessions.Add(newSession);
            }
        }

        allSessions = allSessions.OrderByDescending(s => s.compositeScore).ToList();
        await CloudSaveManager.SaveSessionIndexedAsync(moneyObtained, debtPaid, playTime, compositeScore);
        SaveSessionStats();
    }

    /// <summary>
    /// Saves all session records as a JSON string under one key in PlayerPrefs.
    /// </summary>
    private void SaveSessionStats()
    {
        SessionStatsList wrapper = new SessionStatsList { sessions = allSessions };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(sessionStatsKey, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the session records from PlayerPrefs.
    /// </summary>
    private void LoadSessionStats()
    {
        allSessions.Clear();
        if (PlayerPrefs.HasKey(sessionStatsKey))
        {
            string json = PlayerPrefs.GetString(sessionStatsKey);
            SessionStatsList wrapper = JsonUtility.FromJson<SessionStatsList>(json);
            if (wrapper != null && wrapper.sessions != null)
            {
                allSessions = wrapper.sessions;
            }
        }
    }

    /// <summary>
    /// Returns a copy of the session records.
    /// </summary>
    public List<SessionStats> GetSessionStats() => new List<SessionStats>(allSessions);

    /// <summary>
    /// Clears all stored session data and reloads the current scene.
    /// </summary>
    public void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        allSessions.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
