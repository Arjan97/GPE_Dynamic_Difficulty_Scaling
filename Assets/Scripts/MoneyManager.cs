using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money Settings")]
    [Tooltip("Default starting amount of money (used only if needed).")]
    [SerializeField] private float startingMoney = 100f;
    [Tooltip("Current money (i.e. money obtained during this run, resets each play).")]
    [SerializeField] private float currentMoney = 0f;
    [Tooltip("Current debt amount (resets each play).")]
    [SerializeField] private float currentDebt = 0f;
    [Tooltip("Rate at which debt increases over time (Euros per second).")]
    [SerializeField] private float debtIncreaseRate = 1f;
    [Tooltip("Maximum debt before game over is triggered.")]
    [SerializeField] private float maxDebt = 100f;

    [Header("High Score Settings")]
    [Tooltip("Default high score if none is saved (money obtained target).")]
    [SerializeField] private float defaultHighScore = 100f;
    private float highScore = 0f;

    // NEW: Cumulative totals (persist across plays).
    private float totalMoneyObtained = 0f;
    private float totalDebtPaid = 0f;
    // (PlayTime can be saved separately in LevelController.)

    // UnityEvents to broadcast changes.
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    public FloatEvent OnMoneyChanged;
    public FloatEvent OnDebtChanged;
    public FloatEvent OnNetWorthChanged;

    [Header("Debt Payment UI")]
    [Tooltip("Event used to update the debt payment (mash) slider in the UI.")]
    public FloatEvent OnDebtPaymentProgress;

    // Event raised when debt reaches max.
    public static event Action OnGameOver;

    // PlayerPrefs keys.
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
        // Load high score or use default.
        highScore = PlayerPrefs.GetFloat(HighScoreKey, 0f);
        if (highScore <= 0f)
            highScore = defaultHighScore;

        // Load cumulative totals.
        totalMoneyObtained = PlayerPrefs.GetFloat(TotalMoneyObtainedKey, 0f);
        totalDebtPaid = PlayerPrefs.GetFloat(TotalDebtPaidKey, 0f);

        // Reset current money and debt for new play.
        currentMoney = 0f;
        currentDebt = 0f;

        BroadcastAll();
    }

    void Update()
    {
        // Increase debt continuously.
        IncreaseDebt(debtIncreaseRate * Time.deltaTime);
    }

    /// <summary>
    /// Increases current money and adds to cumulative total.
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
    /// Decreases current money (clamped to 0).
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
    /// Increases current debt.
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
    /// Reduces current debt (clamped to 0) and adds to cumulative debt paid.
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

    public float GetNetWorth() => currentMoney - currentDebt;
    public float GetMoney() => currentMoney;
    public float GetDebt() => currentDebt;
    public float GetMaxDebt() => maxDebt;
    public float GetTotalMoneyObtained() => totalMoneyObtained;
    public float GetTotalDebtPaid() => totalDebtPaid;

    private void BroadcastAll()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    /// <summary>
    /// Saves current session data and cumulative totals to PlayerPrefs.
    /// </summary>
    public void SaveData()
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
    /// Public method to update the debt payment slider via UnityEvent.
    /// </summary>
    public void UpdateDebtPaymentProgress(float progress)
    {
        OnDebtPaymentProgress?.Invoke(progress);
    }
}
