using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class DebtReducer : MonoBehaviour, IDebtReducer
{
    protected float debtReductionAmount;

    public float DebtReductionAmount => debtReductionAmount;

    /// <summary>
    /// Called when the player collects this item. 
    /// Override if you want special effects or animations.
    /// </summary>
    public virtual void ReduceDebt()
    {
        // Apply the reduction to the DebtManager
        DebtManager.Instance.ReduceDebt(debtReductionAmount);

        // Destroy (or disable) this object afterward
        Destroy(gameObject);
    }

    /// <summary>
    /// Check for collision with the player and reduce debt if triggered.
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ReduceDebt();
        }
    }
}
