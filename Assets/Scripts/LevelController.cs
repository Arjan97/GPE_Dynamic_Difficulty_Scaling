using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    // Tracks total playtime in seconds for the current session.
    private float sessionPlayTime = 0f;

    // This event is raised when the game is over.
    public static event Action OnGameOverEvent;

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
        MoneyManager.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.OnGameOver -= HandleGameOver;
        OutsideMapCheck.OnOutsideMapGameOver -= HandleGameOver;
    }

    private void Update()
    {
        // Accumulate playtime during the session.
        sessionPlayTime += Time.deltaTime;
    }

    private void HandleGameOver()
    {
        // Raise the game over event for any subscribers.
        OnGameOverEvent?.Invoke();

        Debug.Log("Game Over! Debt limit reached.");
        Debug.Log("Total playtime: " + sessionPlayTime.ToString("F2") + " seconds");

        // Save playtime to PlayerPrefs.
        PlayerPrefs.SetFloat("PlayTime", sessionPlayTime);

        // Optionally, you can update your high score lists here.
        // For example, assume ScoreManager.Instance is valid and has these methods.
        if (ScoreManager.Instance != null)
        {
            // Update the top-5 lists with this session's cumulative totals.
            ScoreManager.Instance.AddDebtPaidScore(MoneyManager.Instance.GetTotalDebtPaid());
            ScoreManager.Instance.AddMoneyMadeScore(MoneyManager.Instance.GetTotalMoneyObtained());
            ScoreManager.Instance.AddPlayTimeScore(sessionPlayTime);
        }

        // Save MoneyManager data (this saves current money, debt, high score, and cumulative totals).
        MoneyManager.Instance.SaveData(); 

        // Force a save of PlayerPrefs.
        PlayerPrefs.Save();

        // Switch to the "EndScore" scene to display the current session and top 5 scores.
        SceneManager.LoadScene("EndScore");
    }

    /// <summary>
    /// Resets the game (if needed) and reloads the main scene.
    /// </summary>
    private void ResetGame()
    {
        sessionPlayTime = 0f;
        SceneManager.LoadScene("WhiteBOX");
    }
}
