using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class CloudSaveManager : MonoBehaviour
{
    public static CloudSaveManager Instance { get; private set; }
    private const string SessionCountKey = "sessionCount";
    private const string MoneyKeyFormat = "session_{0}_moneyObtained";
    private const string DebtKeyFormat = "session_{0}_debtPaid";
    private const string PlayTimeKeyFormat = "session_{0}_playTime";
    private const string CompositeScoreKeyFormat = "session_{0}_compositeScore";

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
    public async Task SignUpAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }


    /// <summary>
    /// Saves one session’s stats as separate indexed keys (including composite score).
    /// </summary>
    public static async Task SaveSessionIndexedAsync(float moneyObtained, float debtPaid, float playTime, int compositeScore)
    {
        int sessionCount = 0;
        try
        {
            var countData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { SessionCountKey });
            if (countData.TryGetValue(SessionCountKey, out var countItem))
            {
                int.TryParse(countItem.Value.ToString(), out sessionCount);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading session count: " + e.Message);
        }

        int newIndex = sessionCount;
        Dictionary<string, object> dataToSave = new Dictionary<string, object>
        {
            { string.Format(MoneyKeyFormat, newIndex), moneyObtained },
            { string.Format(DebtKeyFormat, newIndex), debtPaid },
            { string.Format(PlayTimeKeyFormat, newIndex), playTime },
            { string.Format(CompositeScoreKeyFormat, newIndex), compositeScore },
            { SessionCountKey, (newIndex + 1).ToString() }
        };

        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
            Debug.Log($"CloudSave: Saved session {newIndex} stats successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError("CloudSave Error: " + e.Message);
        }
    }

    /// <summary>
    /// Loads all session stats from Cloud Save using indexed keys.
    /// </summary>
    public static async Task<List<SessionStats>> LoadAllSessionStatsAsync()
    {
        List<SessionStats> sessions = new List<SessionStats>();
        int sessionCount = 0;
        try
        {
            var countData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { SessionCountKey });
            if (countData.TryGetValue(SessionCountKey, out var countItem))
            {
                int.TryParse(countItem.Value.ToString(), out sessionCount);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading session count: " + e.Message);
        }

        for (int i = 0; i < sessionCount; i++)
        {
            HashSet<string> keys = new HashSet<string>
            {
                string.Format(MoneyKeyFormat, i),
                string.Format(DebtKeyFormat, i),
                string.Format(PlayTimeKeyFormat, i),
                string.Format(CompositeScoreKeyFormat, i)
            };

            try
            {
                Dictionary<string, Unity.Services.CloudSave.Models.Item> sessionData =
                    await CloudSaveService.Instance.Data.Player.LoadAsync(keys);
                if (sessionData.TryGetValue(string.Format(MoneyKeyFormat, i), out var moneyItem) &&
                    sessionData.TryGetValue(string.Format(DebtKeyFormat, i), out var debtItem) &&
                    sessionData.TryGetValue(string.Format(PlayTimeKeyFormat, i), out var playTimeItem) &&
                    sessionData.TryGetValue(string.Format(CompositeScoreKeyFormat, i), out var compositeItem))
                {
                    if (float.TryParse(moneyItem.Value.ToString(), out float moneyObtained) &&
                        float.TryParse(debtItem.Value.ToString(), out float debtPaid) &&
                        float.TryParse(playTimeItem.Value.ToString(), out float playTime) &&
                        int.TryParse(compositeItem.Value.ToString(), out int compositeScore))
                    {
                        // We save the sessionID as the string representation of index.
                        sessions.Add(new SessionStats(i.ToString(), "", moneyObtained, debtPaid, playTime, compositeScore));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading session data for index {i}: {e.Message}");
            }
        }
        return sessions;
    }
}
