using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SessionStats
{
    public string sessionID;
    public float moneyObtained;
    public float debtPaid;
    public float playTime; // in seconds

    public SessionStats(string sessionID, float moneyObtained, float debtPaid, float playTime)
    {
        this.sessionID = sessionID;
        this.moneyObtained = moneyObtained;
        this.debtPaid = debtPaid;
        this.playTime = playTime;
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
    private const int maxSessions = 5;
    private const string sessionStatsKey = "SessionStats";

    // List of all session records loaded from PlayerPrefs.
    private List<SessionStats> allSessions = new List<SessionStats>();

    private void Awake()
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

    /// <summary>
    /// Call this method at the end of a session to add its stats.
    /// </summary>
    public void AddSessionStats(float moneyObtained, float debtPaid, float playTime)
    {
        // Create a new session record with a unique ID.
        string sessionID = Guid.NewGuid().ToString();
        SessionStats newSession = new SessionStats(sessionID, moneyObtained, debtPaid, playTime);
        allSessions.Add(newSession);

        // Sort the sessions:
        // Primary: moneyObtained descending,
        // Secondary: debtPaid descending,
        // Tertiary: playTime descending.
        allSessions = allSessions
            .OrderByDescending(s => s.moneyObtained)
            .ThenByDescending(s => s.debtPaid)
            .ThenByDescending(s => s.playTime)
            .ToList();

        // Keep only the top maxSessions sessions.
        if (allSessions.Count > maxSessions)
            allSessions = allSessions.Take(maxSessions).ToList();

        SaveSessionStats();
    }

    /// <summary>
    /// Saves all session records as a JSON string under one key.
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
    /// Clears all PlayerPrefs saved data and reloads the current scene.
    /// </summary>
    public void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        // Clear in-memory data as well.
        allSessions.Clear();
        // Reload the current scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
