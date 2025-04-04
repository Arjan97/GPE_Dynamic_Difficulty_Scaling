using UnityEngine;

public class CounterAnimationController : MonoBehaviour
{

    [Header("Animator References")]
    [Tooltip("Animator for the money counter UI element.")]
    [SerializeField] private Animator moneyCounterAnimator;
    [Tooltip("Animator for the debt counter UI element.")]
    [SerializeField] private Animator debtCounterAnimator;

    [Header("Animation Triggers")]
    [Tooltip("Trigger name for money increase animation.")]
    [SerializeField] private string moneyIncreaseTrigger = "MoneyIncrease";
    [Tooltip("Trigger name for money decrease animation.")]
    [SerializeField] private string moneyDecreaseTrigger = "MoneyDecrease";
    [Tooltip("Trigger name for debt increase animation.")]
    [SerializeField] private string debtIncreaseTrigger = "DebtIncrease";
    [Tooltip("Trigger name for debt decrease animation.")]
    [SerializeField] private string debtDecreaseTrigger = "DebtDecrease";

    public void PlayMoneyIncreaseAnimation()
    {
        if (moneyCounterAnimator != null)
            moneyCounterAnimator.SetTrigger(moneyIncreaseTrigger);
    }

    public void PlayMoneyDecreaseAnimation()
    {
        if (moneyCounterAnimator != null)
            moneyCounterAnimator.SetTrigger(moneyDecreaseTrigger);
    }

    public void PlayDebtIncreaseAnimation()
    {
        if (debtCounterAnimator != null)
            debtCounterAnimator.SetTrigger(debtIncreaseTrigger);
    }

    public void PlayDebtDecreaseAnimation()
    {
        if (debtCounterAnimator != null)
            debtCounterAnimator.SetTrigger(debtDecreaseTrigger);
    }
}
