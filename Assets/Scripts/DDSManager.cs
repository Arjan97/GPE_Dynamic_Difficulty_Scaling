using UnityEngine;

public enum DifficultyLevel { Easy, Medium, Hard }

public class DDSManager : MonoBehaviour
{
    public static DDSManager Instance { get; private set; }

    [Header("DDS Toggle")]
    [SerializeField] private bool enableDDS = true;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 5f;
    private float timer = 0f;

    [Header("Gamble (Slot Machine) Settings")]
    // For gambling: when debt is high, we want an easier game, so a higher jackpot chance.
    [SerializeField] private float easyJackpotProbability = 0.4f;   // When debt is high (player struggling)
    [SerializeField] private float hardJackpotProbability = 0.1f;     // When debt is low (player doing well)
    // (Medium would fall in between; our interpolation uses hard and easy as extremes.)

    [Header("Player Movement Settings")]
    // For player speeds, we define:
    // - Hard: faster speeds (more challenging) when the player is doing well (low debt).
    // - Easy: slower speeds (easier) when the player is struggling (high debt).
    [SerializeField] private float hardForwardSpeed = 60f;
    [SerializeField] private float easyForwardSpeed = 30f;
    [SerializeField] private float hardRotatingSpeed = 60f;
    [SerializeField] private float easyRotatingSpeed = 30f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        // Get the current debt ratio from MoneyManager.
        // Assume MoneyManager.Instance.GetDebt() returns the current debt
        // and MoneyManager.Instance.MaxDebt returns the maximum debt.
        float currentDebt = MoneyManager.Instance.GetDebt();
        float maxDebt = MoneyManager.Instance.GetMaxDebt();
        float debtRatio = (maxDebt > 0) ? Mathf.Clamp01(currentDebt / maxDebt) : 0f;

        // When debtRatio is 0 (no debt), we want a hard setting (lower jackpot, faster speeds).
        // When debtRatio is 1 (maxed), we want an easy setting (higher jackpot, slower speeds).
        // We'll linearly interpolate:
        float targetJackpotProb = Mathf.Lerp(hardJackpotProbability, easyJackpotProbability, debtRatio);
        // For movement: when debt is high, speeds increase.
        float targetForwardSpeed = Mathf.Lerp(easyForwardSpeed, hardForwardSpeed, debtRatio);
        float targetRotatingSpeed = Mathf.Lerp(easyRotatingSpeed, hardRotatingSpeed, debtRatio);


        Debug.Log($"DDSManager: DebtRatio = {debtRatio:F2} | JackpotProb = {targetJackpotProb:F2} | ForwardSpeed = {targetForwardSpeed} | RotatingSpeed = {targetRotatingSpeed}");

        // Apply these values:
        // Assume SlotMachineOverlay.Instance.SetJackpotProbability(float) updates the slot machine's jackpot chance.
        if (SlotMachineOverlay.Instance != null)
            SlotMachineOverlay.Instance.SetJackpotProbability(targetJackpotProb);

        // Assume InfiniteRunnerMovement.Instance has public setters for forward and rotating speeds.
        if (InfiniteRunnerMovement.Instance != null)
        {
            InfiniteRunnerMovement.Instance.SetForwardSpeed(targetForwardSpeed);
            InfiniteRunnerMovement.Instance.SetRotatingSpeed(targetRotatingSpeed);
        }
    }
}
