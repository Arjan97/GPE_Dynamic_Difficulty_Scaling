using UnityEngine;
using System;

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

    // Event raised when the debt reaches the max threshold.
    public static event Action OnGameOver;

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
    }
}
