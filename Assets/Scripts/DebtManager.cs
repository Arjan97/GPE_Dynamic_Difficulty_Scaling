using UnityEngine;
using System;
using UnityEngine.Events;

public class DebtManager : MonoBehaviour
{
    // Singleton instance for easy access.
    public static DebtManager Instance { get; private set; }

    [Header("Debt Settings")]
    [Tooltip("Current debt amount in Euros.")]
    public float currentDebt = 0f;
    [Tooltip("Debt threshold before game over is triggered.")]
    public float maxDebt = 100f;
    [Tooltip("Rate at which debt increases (Euros per second).")]
    public float debtIncreaseRate = 1f;
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    // Event raised when the debt reaches the max threshold.
    public static event Action OnGameOver;
    // This UnityEvent passes a float (the current debt) to listeners.
    public FloatEvent OnDebtChanged;

    void Awake()
    {
        // Implement a simple Singleton pattern.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        // Increase debt continuously over time.
        currentDebt += debtIncreaseRate * Time.deltaTime;

        // Check if debt has reached the game over threshold.
        if (currentDebt >= maxDebt)
        {
            currentDebt = maxDebt; // Optionally clamp the debt value.
            // Raise the game over event.
            OnGameOver?.Invoke();
            // Optionally disable further accumulation.
            enabled = false;
        }
        // Fire the event so any listeners (UI, etc.) update
        OnDebtChanged?.Invoke(currentDebt);
    }

    /// <summary>
    /// Reduces the current debt by the specified amount.
    /// </summary>
    /// <param name="amount">The Euro amount to subtract from the debt.</param>
    public void ReduceDebt(float amount)
    {
        currentDebt -= amount;
        if (currentDebt < 0f)
        {
            currentDebt = 0f;
        }
        // Fire the event to update UI
        OnDebtChanged?.Invoke(currentDebt);
    }
}
