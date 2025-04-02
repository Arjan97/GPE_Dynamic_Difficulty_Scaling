using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class DebtPaymentOverlay : MonoBehaviour
{
    public static DebtPaymentOverlay Instance { get; private set; }

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
    [SerializeField] private Slider progressSlider;
    [Tooltip("Input field to set the required mash count (e.g., '20').")]
    [SerializeField] private TMP_InputField mashTimesInput;
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

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        overlayPanel.SetActive(false);
    }

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = goodMashThreshold;
        }
        if (timerFillImage != null)
            timerFillImage.fillAmount = 1f;

        if (mashTimesInput != null && !string.IsNullOrEmpty(mashTimesInput.text))
        {
            if (int.TryParse(mashTimesInput.text, out int parsedValue))
            {
                goodMashThreshold = parsedValue;
                if (progressSlider != null)
                    progressSlider.maxValue = goodMashThreshold;
            }
        }
    }

    /// <summary>
    /// Shows the overlay and starts the mini-game.
    /// </summary>
    public void ShowOverlay()
    {
        if (SlotMachineOverlay.Instance != null && SlotMachineOverlay.Instance.IsActive)
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
        MoneyManager.Instance.DecreaseMoney(basePayment);

        resultText.text = $"Paid off €{debtReduction:F2} and used €{basePayment:F2}!";
        StartCoroutine(HideOverlayAfterDelay(2f));
    }

    private IEnumerator HideOverlayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        overlayPanel.SetActive(false);
    }

    /// <summary>
    /// Updates the progress slider and invokes the progress update event.
    /// </summary>
    private void UpdateProgress(float progress)
    {
        OnProgressUpdate?.Invoke(progress);
        if (progressSlider != null)
            progressSlider.value = progress;
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
