using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelController : MonoBehaviour
{
    public static LevelController Instance { get; private set; }
    // Tracks total playtime in seconds.
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
        // Accumulate playtime
        sessionPlayTime += Time.deltaTime;
    }

    private void HandleGameOver()
    {
        // Raise the event so any interested subscribers can react.
        OnGameOverEvent?.Invoke();

        Debug.Log("Game Over! Debt limit reached.");
        Debug.Log("Total playtime: " + sessionPlayTime.ToString("F2") + " seconds");

        // Switch to the "GameOver" scene.
        SceneManager.LoadScene("GameOver");
    }
    private void ResetGame() {         
           sessionPlayTime = 0f;
           SceneManager.LoadScene("WhiteBOX");
       }
}
