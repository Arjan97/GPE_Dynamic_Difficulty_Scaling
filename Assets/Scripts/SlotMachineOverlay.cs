using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SlotMachineOverlay : MonoBehaviour
{

    [Header("UI References")]
    [Tooltip("The overlay panel that contains the slot machine UI. Set inactive by default.")]
    [SerializeField] private GameObject panel;
    [Tooltip("Button for wagering all the current money.")]
    [SerializeField] private Button allInButton;
    [Tooltip("Button for wagering half the current money.")]
    [SerializeField] private Button halfButton;
    [Tooltip("Button for closing the slot machine overlay.")]
    [SerializeField] private Button closeButton;
    [Tooltip("Text field to display the gamble outcome (multiplier and win/lose amount).")]
    [SerializeField] private TMP_Text outcomeText;

    [Header("Gamble Settings")]
    [Tooltip("Chance for a jackpot outcome (5x multiplier).")]
    [SerializeField, Range(0f, 1f)] private float jackpotProbability = 0.05f;
    [Tooltip("Chance to lose everything (0x multiplier).")]
    [SerializeField, Range(0f, 1f)] private float loseAllProbability = 0.20f;
    // The remaining probability yields an average outcome between 0.5x and 2x.
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip jackpotClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private DebtPaymentOverlay debtPaymentOverlay;

    private float jackpotBonusModifier = 0f;
   
    void Start()
    {
        panel.SetActive(false);

        if (debtPaymentOverlay == null)
            debtPaymentOverlay = FindFirstObjectByType<DebtPaymentOverlay>();
        allInButton.onClick.AddListener(PlayAllIn);
        halfButton.onClick.AddListener(PlayHalf);
        closeButton.onClick.AddListener(HideSlotMachine);
    }

    public void SetJackpotBoost(float modifier)
    {
        jackpotBonusModifier = modifier;
    }

    /// <summary>
    /// Displays the slot machine overlay and enables the HUD buttons.
    /// </summary>
    public void ShowSlotMachine()
    {
        if (debtPaymentOverlay != null && debtPaymentOverlay.IsActive && !IsActive)
            return; // Don't show if debt overlay is ac or itselftive

        panel.SetActive(true);
        outcomeText.text = "Gamble to win up to 5x! Warning: You may lose it all.";
        SetButtonsInteractable(true);
    }


    /// <summary>
    /// Hides the slot machine overlay.
    /// </summary>
    public void HideSlotMachine()
    {
        panel.SetActive(false);
    }

    /// <summary>
    /// Enables or disables the HUD buttons.
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        if (allInButton != null) allInButton.interactable = interactable;
        if (halfButton != null) halfButton.interactable = interactable;
        if (closeButton != null) closeButton.interactable = interactable;
    }

    private void PlayAllIn()
    {
        float bet = MoneyManager.Instance.GetMoney();
        PlayGamble(bet);
    }

    private void PlayHalf()
    {
        float bet = MoneyManager.Instance.GetMoney() * 0.5f;
        PlayGamble(bet);
    }

    /// <summary>
    /// Public function to display a "No money to gamble!" message.
    /// This function shows the message briefly and then hides the overlay.
    /// </summary>
    public void DisplayNoMoneyMessage()
    {
        if (panel.activeSelf)
            return; // Don't show if already shown
        outcomeText.text = "No money to gamble!";
        panel.SetActive(true);  
        SetButtonsInteractable(false);
        StartCoroutine(HideAfterDelay(1.0f));
    }
    /// <summary>
    /// Executes the gamble: if no money, shows "Not enough money!" and hides the overlay;
    /// otherwise, disables the HUD buttons, deducts the bet, calculates the outcome, updates money,
    /// displays the result, and hides the overlay after a delay.
    /// </summary>
    /// <param name="bet">The wager amount.</param>
    private void PlayGamble(float bet)
    {
        // If the player has no money, display "Not enough money!".
        if (bet <= 0)
        {
            outcomeText.text = "Not enough money!";
            SetButtonsInteractable(false);
            StartCoroutine(HideAfterDelay(1f));
            return;
        }

        // Disable the buttons while processing the gamble.
        SetButtonsInteractable(false);

        // Deduct the wager.
        MoneyManager.Instance.DecreaseMoney(bet, false);

        // Determine outcome.
        float rand = Random.value;
        float multiplier;
        float finalJackpotChance = jackpotProbability + jackpotBonusModifier;
        if (rand < finalJackpotChance)
        {
            multiplier = 5f;
            PlaySound(jackpotClip);
        }
        else if (rand < jackpotProbability + loseAllProbability)
        {
            multiplier = 0f;
            PlaySound(loseClip);
        }

        else
        {
            multiplier = Random.Range(1.1f, 2f);
            PlaySound(winClip);
        }

        float payout = bet * multiplier;
        if (payout >= 0)
        MoneyManager.Instance.IncreaseMoney(payout);
        else
            MoneyManager.Instance.DecreaseMoney(Mathf.Abs(payout), true);

        float net = payout - bet;
        string result = net >= 0 ? $"Win: €{net:F2}" : $"Lose: €{Mathf.Abs(net):F2}";
        outcomeText.text = $"Multiplier: {multiplier:F1}x\n{result}";

        // Hide overlay after 2 seconds.
        StartCoroutine(HideAfterDelay(2.0f));
    }
    public bool IsActive => panel.activeSelf;
    public void SetLoseAllProbability(float newProb)
    {
        loseAllProbability = newProb;
    }
    public void SetJackpotProbability(float newProbability)
    {
        jackpotProbability = newProbability;
    }
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
        // Re-enable buttons so next use starts with interactable buttons.
        SetButtonsInteractable(true);
    }
}
