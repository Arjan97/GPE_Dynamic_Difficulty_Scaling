using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using static UnityEngine.Rendering.DebugUI;

public class DebtPaymentOverlay : MonoBehaviour
{

    [Header("UI References")]
    [Tooltip("The overlay panel for the debt payment mini-game.")]
    [SerializeField] private GameObject overlayPanel;
    [Tooltip("Instruction text (e.g., 'Mash the button!').")]
    [SerializeField] private TMP_Text instructionText;
    [Tooltip("Text that displays the current mash count (e.g., 'Mashed X times!').")]
    [SerializeField] private TMP_Text mashCountText;
    [Tooltip("Result text displaying the final debt paid.")]
    [SerializeField] private TMP_Text resultText;
    [Tooltip("Slider that shows mash progress (range 0 to mash threshold).")]
    [SerializeField] private GameObject progressSlider;
    [Tooltip("Image used as a timer fill component. FillAmount goes from 1 to 0.")]
    [SerializeField] private Image timerFillImage;

    [Header("Mini-Game Settings")]
    [Tooltip("Percentage of player's current money to use as base payment (e.g., 0.5 for half).")]
    [SerializeField] private float basePaymentPercent = 0.5f;
    [Tooltip("Duration of the button mash mini-game (in seconds).")]
    [SerializeField] private float mashDuration = 5f;
    [Tooltip("Number of button presses required for a 'good' mash. Default is 20.")]
    [SerializeField] private int goodMashThreshold = 20;
    [Tooltip("Bonus multiplier if mash count is high.")]
    [SerializeField] private float bonusMultiplier = 1.5f;
    [Tooltip("Multiplier if mash count is low.")]
    [SerializeField] private float poorMultiplier = 0.8f;

    [Header("Events")]
    [Tooltip("Event that updates mash progress (value from 0 to mash threshold).")]
    public UnityEvent<float> OnProgressUpdate;

    private int mashCount = 0;
    private bool isMashing = false;
    private float lockedInMoneyAtStart = 0f;
    [SerializeField] private SlotMachineOverlay slotMachine;

    private void Start()
    {
        // Ensure the overlay panel is inactive at the start.
        overlayPanel.SetActive(false);
        if (slotMachine == null)
            slotMachine = FindFirstObjectByType<SlotMachineOverlay>();

     
        if (timerFillImage != null)
            timerFillImage.fillAmount = 1f;
    }

    /// <summary>
    /// Shows the overlay and starts the mini-game.
    /// </summary>
    public void ShowOverlay()
    {
        if (slotMachine != null && slotMachine.IsActive && !IsActive)
            return; // Block if slot machine is open

        overlayPanel.SetActive(true);
        instructionText.text = "Mash the button!";
        resultText.text = "";
        UpdateProgress(0);
        UpdateMashCountText(0);
        if (timerFillImage != null)
            timerFillImage.fillAmount = 1f;

        lockedInMoneyAtStart = MoneyManager.Instance.GetMoney();
        mashCount = 0;
        StartCoroutine(MashMiniGame());
    }

    public bool IsActive => overlayPanel.activeSelf;

    private void Update()
    {
        // When the overlay is active, register a mash on any click or tap.
        if (overlayPanel.activeSelf && isMashing)
        {
            // For mouse input
            if (Input.GetMouseButtonDown(0))
            {
                RegisterMash();
            }
            // For touch input (register on first touch phase)
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                RegisterMash();
            }
        }
    }
    public void DisplayNoMoneyMessage()
    {
        if (overlayPanel.activeSelf)
            return; // Don't show if already shown
        resultText.text = "";
        instructionText.text = "No money to pay with, go gamble!";
        overlayPanel.SetActive(true);
        DisableSlider(true);
        timerFillImage.fillAmount = 0f;
        StartCoroutine(HideOverlayAfterDelay(1.0f));
    }

    private void DisableSlider(bool disable)
    {
        if (progressSlider != null)
            progressSlider.gameObject.SetActive(!disable);
        StartCoroutine(EnableSliderAfterDelay(1f));
    }

    private IEnumerator EnableSliderAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (progressSlider != null)
            progressSlider.gameObject.SetActive(true);
    }
    private IEnumerator MashMiniGame()
    {
        isMashing = true;
        float timer = 0f;
        while (timer < mashDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp(mashCount, 0, goodMashThreshold);
            UpdateProgress(progress);
            UpdateMashCountText(mashCount);

            if (timerFillImage != null)
                timerFillImage.fillAmount = 1f - (timer / mashDuration);

            yield return null;
        }
        isMashing = false;
        EvaluateMash();
    }

    /// <summary>
    /// Registers a mash input by incrementing the mash counter.
    /// </summary>
    public void RegisterMash()
    {
        if (isMashing)
        {
            mashCount++;
            UpdateMashCountText(mashCount);
        }
    }

    /// <summary>
    /// Evaluates mash performance, computes debt reduction, updates MoneyManager, and displays the result.
    /// </summary>
    private void EvaluateMash()
    {
        float multiplier = (mashCount >= goodMashThreshold) ? bonusMultiplier : poorMultiplier;
        float basePayment = lockedInMoneyAtStart * basePaymentPercent;
        float debtReduction = basePayment * multiplier;

        MoneyManager.Instance.ReduceDebt(debtReduction);
        MoneyManager.Instance.DecreaseMoney(basePayment, true);
        instructionText.text = "";
        resultText.text = $"Paid off €{debtReduction:F2} and used €{basePayment:F2}!";
        StartCoroutine(HideOverlayAfterDelay(2f));
    }

    private IEnumerator HideOverlayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        overlayPanel.SetActive(false);
        instructionText.text = "";
        resultText.text = "";
        timerFillImage.fillAmount = 0f;
    }

    /// <summary>
    /// Updates the progress slider and invokes the progress update event.
    /// </summary>
    private void UpdateProgress(float progress)
    {
        OnProgressUpdate?.Invoke(progress);
    }

    /// <summary>
    /// Updates the text field that displays the current mash count.
    /// </summary>
    private void UpdateMashCountText(int count)
    {
        if (mashCountText != null)
            mashCountText.text = $"Mashed {count} times!";
    }
}
