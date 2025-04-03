using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }

    [Header("Money Settings")]
    [Tooltip("Current money in this session.")]
    [SerializeField] private float currentMoney = 0f;
    [Tooltip("Current debt in this session.")]
    [SerializeField] private float currentDebt = 0f;
    [Tooltip("Rate at which debt increases per second.")]
    [SerializeField] private float debtIncreaseRate = 1f;
    [Tooltip("Max debt before game over.")]
    [SerializeField] private float maxDebt = 100f;

    [Header("High Score Settings")]
    [Tooltip("Default high score if none is saved.")]
    [SerializeField] private float defaultHighScore = 100f;
    private float highScore = 0f;

    // Session-based totals (reset each new play).
    private float sessionMoneyObtained = 0f;
    private float sessionDebtPaid = 0f;

    // Events to broadcast changes to UI, if needed.
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    public FloatEvent OnMoneyChanged;
    public FloatEvent OnDebtChanged;
    public FloatEvent OnNetWorthChanged;

    [Header("Debt Payment UI")]
    [Tooltip("Event to update a mash slider in the UI.")]
    public FloatEvent OnDebtPaymentProgress;

    // Game over event
    public static event Action OnGameOver;

    // PlayerPrefs keys for optional persistent data.
    private const string HighScoreKey = "HighScore";

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

    private void Start()
    {
        // Load high score or use default.
        highScore = PlayerPrefs.GetFloat(HighScoreKey, 0f);
        if (highScore <= 0f)
            highScore = defaultHighScore;

        // Reset current money/debt and session totals for a new run.
        currentMoney = 0f;
        currentDebt = 0f;
        sessionMoneyObtained = 0f;
        sessionDebtPaid = 0f;

        BroadcastAll();
    }

    private void Update()
    {
        // Increase debt over time.
        IncreaseDebt(debtIncreaseRate * Time.deltaTime);
    }

    /// <summary>
    /// Increases current money by amount and updates session total money obtained.
    /// </summary>
    public void IncreaseMoney(float amount)
    {
        currentMoney += amount;
        sessionMoneyObtained += amount;

        OnMoneyChanged?.Invoke(currentMoney);
        OnNetWorthChanged?.Invoke(GetNetWorth());

        // Update a "high score" if you want to track largest session money in a single run.
        if (currentMoney > highScore)
        {
            highScore = currentMoney;
            PlayerPrefs.SetFloat(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Decreases current money by amount (clamped to 0).
    /// </summary>
    public void DecreaseMoney(float amount)
    {
        currentMoney -= amount;
        if (currentMoney < 0f)
            currentMoney = 0f;

        OnMoneyChanged?.Invoke(currentMoney);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }
    public void ResetSession()
    {
        // Reset session-specific values (but leave cumulative persistent data intact)
        currentMoney = 0f;
        currentDebt = 0f;
        // Reset session totals:
        sessionMoneyObtained = 0f;
        sessionDebtPaid = 0f;

        // Broadcast new (reset) values so that any UI updates immediately.
        BroadcastAll();
    }

    /// <summary>
    /// Increases current debt by amount (clamped at maxDebt => triggers game over).
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
    /// Reduces current debt by amount (clamped to 0) and updates session debt paid.
    /// </summary>
    public void ReduceDebt(float amount)
    {
        currentDebt -= amount;
        if (currentDebt < 0f)
            currentDebt = 0f;

        sessionDebtPaid += MathF.Abs(amount);

        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    public float GetNetWorth() => currentMoney - currentDebt;
    public float GetMoney() => currentMoney;
    public float GetDebt() => currentDebt;
    public float GetMaxDebt() => maxDebt;

    public float GetSessionMoneyObtained() => sessionMoneyObtained;
    public float GetSessionDebtPaid() => sessionDebtPaid;

    private void BroadcastAll()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnDebtChanged?.Invoke(currentDebt);
        OnNetWorthChanged?.Invoke(GetNetWorth());
    }

    // Optionally, if you want to store the final session values in PlayerPrefs,
    // you can do so in a public method or OnDestroy() etc.
    public void SaveData()
    {
        // For example, if you want to keep track of high score:
        PlayerPrefs.SetFloat(HighScoreKey, highScore);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit() => SaveData();
    private void OnDestroy() => SaveData();

    /// <summary>
    /// Updates a debt payment slider in the UI.
    /// </summary>
    public void UpdateDebtPaymentProgress(float progress)
    {
        OnDebtPaymentProgress?.Invoke(progress);
    }
}