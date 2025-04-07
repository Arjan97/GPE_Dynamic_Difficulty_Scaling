using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }

    // Tracks total playtime in seconds for the current session.
    private float sessionPlayTime = 0f;
    private bool gameOverHandled = false;

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
    }

    private void OnDisable()
    {
        OutsideMapCheck.OnOutsideMapGameOver -= HandleGameOver;
    }

    private void Update()
    {
        // Accumulate playtime during the session.
        sessionPlayTime += Time.deltaTime;
    }

    public void HandleGameOver()
    {
        if (gameOverHandled) return;
        gameOverHandled = true;
        // Raise the game over event for any subscribers.
        OnGameOverEvent?.Invoke();
        if (InfiniteRunnerMovement.Instance != null)
        {
            InfiniteRunnerMovement.Instance.GameOver(true);
        }
        Debug.Log("Game Over! Debt limit reached.");
        Debug.Log("Total playtime: " + sessionPlayTime.ToString("F2") + " seconds");

        // Save playtime to PlayerPrefs.
        PlayerPrefs.SetFloat("PlayTime", sessionPlayTime);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddSessionStats(
            MoneyManager.Instance.GetSessionMoneyObtained(),
            MoneyManager.Instance.GetSessionDebtPaid(),
            sessionPlayTime);
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
    public void ResetGame()
    {
        // Reset MoneyManager session values.
        if (MoneyManager.Instance != null)
        {
            MoneyManager.Instance.ResetSession();
        }
        if (InfiniteRunnerMovement.Instance != null)
        {
            InfiniteRunnerMovement.Instance.GameOver(false);
        }
        sessionPlayTime = 0f;
        gameOverHandled = false;
        SceneManager.LoadScene("WhiteBOX");
    }
}
