using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money Settings")]
    [Tooltip("Default starting amount of money (used only if needed).")]
    [SerializeField] private float startingMoney = 100f;
    [Tooltip("Current money (i.e. money paid during this run, resets each play).")]
    [SerializeField] private float currentMoney = 0f;
    [Tooltip("Current debt amount (resets each play).")]
    [SerializeField] private float currentDebt = 0f;
    [Tooltip("Rate at which debt increases over time (Euros per second).")]
    [SerializeField] private float debtIncreaseRate = 1f;
    [Tooltip("Maximum debt before game over is triggered.")]
    [SerializeField] private float maxDebt = 100f;

    [Header("High Score Settings")]
    [Tooltip("Default high score if none is saved (money paid target).")]
    [SerializeField] private float defaultHighScore = 100f;
    // High score persists between plays.
    private float highScore = 0f;

    // UnityEvents to broadcast changes.
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    public FloatEvent OnMoneyChanged;
    public FloatEvent OnDebtChanged;
    public FloatEvent OnNetWorthChanged; // (Optional)

    // Event raised when the debt reaches the max threshold.
    public static event Action OnGameOver;

    private const string MoneyKey = "CurrentMoney";
    private const string DebtKey = "CurrentDebt";
    private const string HighScoreKey = "HighScore";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load the saved high score, or use the default if none exists.
        highScore = PlayerPrefs.GetFloat(HighScoreKey, 0f);
        if (highScore <= 0f)
            highScore = defaultHighScore;

        // Reset current money and debt each play.
        currentMoney = 0f;
        currentDebt = 0f;

        BroadcastAll();
    }

    void Update()
    {
        // Increase debt continuously (simulate interest/pressure).
        IncreaseDebt(debtIncreaseRate * Time.deltaTime);
    }

    /// <summary>
    /// Increases the player's money by the given amount (money paid during the run).
    /// </summary>
    public void IncreaseMoney(float amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
        OnNetWorthChanged?.Invoke(GetNetWorth());

        // If the new score exceeds the current high score, update it.
        if (currentMoney > highScore)
        {
            highScore = currentMoney;
            PlayerPrefs.SetFloat(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Decreases the player's money by the given amount (clamped to zero).
    /// </summary>
    public void DecreaseMoney(float amount)
    {
        currentMoney -= amount;
        if (currentMoney < 0f)
            currentMoney = 0f;
        OnMoneyChanged?.Invoke(currentMoney);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    /// <summary>
    /// Increases the player's debt by the given amount.
    /// </summary>
    public void IncreaseDebt(float amount)
    {
        currentDebt += amount;
        if (currentDebt > maxDebt)
        {
            currentDebt = maxDebt;
            OnGameOver?.Invoke();

        }
        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    /// <summary>
    /// Reduces the player's debt by the given amount (clamped to zero).
    /// </summary>
    public void ReduceDebt(float amount)
    {
        currentDebt -= amount;
        if (currentDebt < 0f)
            currentDebt = 0f;
        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    /// <summary>
    /// Returns the player's net worth (current money minus current debt).
    /// </summary>
    public float GetNetWorth()
    {
        return currentMoney - currentDebt;
    }

    /// <summary>
    /// Returns the player's current money.
    /// </summary>
    public float GetMoney()
    {
        return currentMoney;
    }
    /// <summary>
    /// Returns the player's current debt.
    /// </summary>
    public float GetDebt()
    {
        return currentDebt;
    }


    /// <summary>
    /// Broadcasts current money, debt, and (optionally) net worth.
    /// </summary>
    private void BroadcastAll()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }
    /// <summary>
    /// Saves current money, debt, and high score to PlayerPrefs.
    /// </summary>
    private void SaveData()
    {
        PlayerPrefs.SetFloat(MoneyKey, currentMoney);
        PlayerPrefs.SetFloat(DebtKey, currentDebt);
        PlayerPrefs.SetFloat(HighScoreKey, highScore);
        PlayerPrefs.Save();
    }

    void OnApplicationQuit() => SaveData();
    void OnDestroy() => SaveData();
}
