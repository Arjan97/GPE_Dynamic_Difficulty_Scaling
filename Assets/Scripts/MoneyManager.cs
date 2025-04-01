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
    private float highScore = 0f;

    // NEW: Cumulative totals that persist between plays.
    private float totalMoneyObtained = 0f;
    private float totalDebtPaid = 0f;
    // (PlayTime can be saved by your LevelController.)

    // UnityEvents to broadcast changes.
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    public FloatEvent OnMoneyChanged;
    public FloatEvent OnDebtChanged;
    public FloatEvent OnNetWorthChanged;

    [Header("Debt Payment UI")]
    [Tooltip("Event used to update the mash slider in the debt payment UI.")]
    public FloatEvent OnDebtPaymentProgress;

    // Event raised when the debt reaches the max threshold.
    public static event Action OnGameOver;

    // PlayerPrefs keys
    private const string MoneyKey = "CurrentMoney";
    private const string DebtKey = "CurrentDebt";
    private const string HighScoreKey = "HighScore";
    private const string TotalMoneyObtainedKey = "TotalMoneyObtained";
    private const string TotalDebtPaidKey = "TotalDebtPaid";

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

        // Load cumulative totals.
        totalMoneyObtained = PlayerPrefs.GetFloat(TotalMoneyObtainedKey, 0f);
        totalDebtPaid = PlayerPrefs.GetFloat(TotalDebtPaidKey, 0f);

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
    /// Increases the player's money by the given amount and adds that to the cumulative total.
    /// </summary>
    public void IncreaseMoney(float amount)
    {
        currentMoney += amount;
        totalMoneyObtained += amount;
        OnMoneyChanged?.Invoke(currentMoney);
        OnNetWorthChanged?.Invoke(GetNetWorth());

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
    /// Reduces the player's debt by the given amount (clamped to zero) and adds to the cumulative debt paid.
    /// </summary>
    public void ReduceDebt(float amount)
    {
        currentDebt -= amount;
        if (currentDebt < 0f)
            currentDebt = 0f;
        totalDebtPaid += amount;
        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    /// <summary>
    /// Returns the player's net worth (current money minus current debt).
    /// </summary>
    public float GetNetWorth() => currentMoney - currentDebt;

    /// <summary>
    /// Returns the player's current money.
    /// </summary>
    public float GetMoney() => currentMoney;

    /// <summary>
    /// Returns the player's current debt.
    /// </summary>
    public float GetDebt() => currentDebt;

    public float GetMaxDebt() => maxDebt;

    /// <summary>
    /// Returns the total money obtained (cumulative).
    /// </summary>
    public float GetTotalMoneyObtained() => totalMoneyObtained;

    /// <summary>
    /// Returns the total debt paid (cumulative).
    /// </summary>
    public float GetTotalDebtPaid() => totalDebtPaid;

    /// <summary>
    /// Broadcasts current money, debt, and net worth.
    /// </summary>
    private void BroadcastAll()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    /// <summary>
    /// Saves current money, debt, high score, and cumulative totals to PlayerPrefs.
    /// </summary>
    private void SaveData()
    {
        PlayerPrefs.SetFloat(MoneyKey, currentMoney);
        PlayerPrefs.SetFloat(DebtKey, currentDebt);
        PlayerPrefs.SetFloat(HighScoreKey, highScore);
        PlayerPrefs.SetFloat(TotalMoneyObtainedKey, totalMoneyObtained);
        PlayerPrefs.SetFloat(TotalDebtPaidKey, totalDebtPaid);
        PlayerPrefs.Save();
    }

    void OnApplicationQuit() => SaveData();
    void OnDestroy() => SaveData();

    /// <summary>
    /// Public function that can be called to update the debt payment slider.
    /// </summary>
    public void UpdateDebtPaymentProgress(float progress)
    {
        OnDebtPaymentProgress?.Invoke(progress);
    }
}
