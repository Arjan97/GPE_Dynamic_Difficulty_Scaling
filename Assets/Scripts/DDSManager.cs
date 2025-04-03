using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

public class DDSManager : MonoBehaviour
{

    [Header("DDS Toggle")]
    [SerializeField] private bool enableDDS = true;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 5f;
    private float timer = 0f;

    [Header("Gamble (Slot Machine) Settings")]
    // Base settings using debt ratio:
    [SerializeField] private float easyJackpotProbability = 0.4f;   // When debt is high (player struggling)
    [SerializeField] private float hardJackpotProbability = 0.1f;     // When debt is low (player doing well)

    [Header("Player Movement Settings")]
    [SerializeField] private float hardForwardSpeed = 90f;
    [SerializeField] private float easyForwardSpeed = 50f;
    [SerializeField] private float hardRotatingSpeed = 90f;
    [SerializeField] private float easyRotatingSpeed = 50f;

    [Header("Money-Based Difficulty Adjustment")]
    [Tooltip("If player's money (as a fraction of maxDebt) exceeds this threshold, the jackpot probability is further reduced.")]
    [SerializeField] private float moneyThresholdRatio = 0.5f;  // if current money > 50% of maxDebt.
    [Tooltip("The jackpot probability to use when the player is 'rich'.")]
    [SerializeField] private float richJackpotProbability = 0.1f;

    [Tooltip("Lose-all chance when player is struggling (easy).")]
    [SerializeField] private float easyLoseAllProbability = 0.1f;

    [Tooltip("Lose-all chance when player is doing well (hard).")]
    [SerializeField] private float hardLoseAllProbability = 0.6f;

    [Tooltip("If player's money exceeds the threshold, increase lose-all chance.")]
    [SerializeField] private float richLoseAllProbability = 0.6f;
    [SerializeField] private SlotMachineOverlay slotMachineOverlay;

    private void Start()
    {
        if (slotMachineOverlay == null)
            slotMachineOverlay = FindFirstObjectByType<SlotMachineOverlay>();
    }

    void Update()
    {
        if (!enableDDS)
            return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            UpdateDifficulty();
            timer = 0f;
        }
    }

    private void UpdateDifficulty()
    {
        // Retrieve current debt and max debt from MoneyManager.
        float currentDebt = MoneyManager.Instance.GetDebt();
        float maxDebt =     MoneyManager.Instance.GetMaxDebt();
        float debtRatio = (maxDebt > 0) ? Mathf.Clamp01(currentDebt / maxDebt) : 0f;

        // Base interpolation based on debt ratio:
        float targetJackpotProb = Mathf.Lerp(hardJackpotProbability, easyJackpotProbability, debtRatio);
        float targetForwardSpeed = Mathf.Lerp(easyForwardSpeed, hardForwardSpeed, debtRatio);
        float targetRotatingSpeed = Mathf.Lerp(easyRotatingSpeed, hardRotatingSpeed, debtRatio);
        float targetLoseAllProb = Mathf.Lerp(easyLoseAllProbability, hardLoseAllProbability, debtRatio);

        // Further adjust jackpot probability based on player's money:
        float currentMoney = MoneyManager.Instance.GetMoney();
        float moneyRatio = (maxDebt > 0) ? Mathf.Clamp01(currentMoney / maxDebt) : 0f;
        if (moneyRatio > moneyThresholdRatio)
        {
            // Interpolate from the current targetJackpotProb to richJackpotProbability
            // as moneyRatio goes from moneyThresholdRatio to 1.
            float t = (moneyRatio - moneyThresholdRatio) / (1f - moneyThresholdRatio);
            targetJackpotProb = Mathf.Lerp(targetJackpotProb, richJackpotProbability, t);
            targetLoseAllProb = Mathf.Lerp(targetLoseAllProb, richLoseAllProbability, t);
        }

        Debug.Log($"DDSManager: DebtRatio = {debtRatio:F2}, MoneyRatio = {moneyRatio:F2} => JackpotProb = {targetJackpotProb:F2}, hardLoseAllProb = {hardLoseAllProbability:F2}, RichLoseAllProb = {richLoseAllProbability:F2}, ForwardSpeed = {targetForwardSpeed}, RotatingSpeed = {targetRotatingSpeed}");

        // Apply these values:
        if (slotMachineOverlay != null)
            slotMachineOverlay.SetJackpotProbability(targetJackpotProb);
            slotMachineOverlay.SetLoseAllProbability(targetLoseAllProb);


        if (InfiniteRunnerMovement.Instance != null)
        {
            InfiniteRunnerMovement.Instance.SetForwardSpeed(targetForwardSpeed);
            InfiniteRunnerMovement.Instance.SetRotatingSpeed(targetRotatingSpeed);
        }
    }
}
