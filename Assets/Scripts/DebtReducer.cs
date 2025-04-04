using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class DebtReducer : MonoBehaviour, IDebtReducer
{
    protected float debtAmount;

    [Tooltip("If true, this pickup will add debt instead of reducing it.")]
    [SerializeField] protected bool isDebtAddition = false;

    public float DebtReductionAmount => debtAmount;

    /// <summary>
    /// Reduces debt and adds score (only for reducing pickups).
    /// </summary>
    public virtual void ReduceDebt()
    {
        //MoneyManager.Instance.ReduceDebt(debtAmount);
        MoneyManager.Instance.IncreaseMoney(debtAmount);

        Destroy(gameObject);
    }

    /// <summary>
    /// Adds debt (and does not affect score).
    /// </summary>
    public virtual void AddDebt()
    {
        MoneyManager.Instance.IncreaseDebt(debtAmount, true);

        Destroy(gameObject);
    }

    /// <summary>
    /// On collision with the player, check the flag and call the proper method.
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isDebtAddition)
            {
                AddDebt();
            }
            else
            {
                ReduceDebt();
            }
        }
    }
}
