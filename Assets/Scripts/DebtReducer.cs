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
        DebtManager.Instance.ReduceDebt(debtAmount);
        ScoreManager.Instance.AddScore(debtAmount);

        Destroy(gameObject);
    }

    /// <summary>
    /// Adds debt (and does not affect score).
    /// </summary>
    public virtual void AddDebt()
    {
        // Passing a negative value increases the debt
        DebtManager.Instance.ReduceDebt(-debtAmount);

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
