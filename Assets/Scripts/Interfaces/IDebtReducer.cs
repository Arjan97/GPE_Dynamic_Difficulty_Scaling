/// <summary>
/// Anything that can reduce the player's debt should implement this interface.
/// </summary>
public interface IDebtReducer
{
    /// <summary>
    /// How much this item (or script) reduces debt.
    /// </summary>
    float DebtReductionAmount { get; }

    /// <summary>
    /// Called to actually reduce the debt (and optionally destroy itself).
    /// </summary>
    void ReduceDebt();
}
