using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DebtPayment : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get current money and current debt from the MoneyManager singleton.
            float moneyAvailable = MoneyManager.Instance.GetMoney();
            float currentDebt = MoneyManager.Instance.GetDebt();

            // Determine the payment amount (the lesser of the two).
            float payment = Mathf.Min(moneyAvailable, currentDebt);

            if (payment > 0)
            {
                // Use the player's money to reduce the debt.
                MoneyManager.Instance.ReduceDebt(payment);
                MoneyManager.Instance.DecreaseMoney(payment);
            }

            // Destroy this trigger so it can't be used again.
            Destroy(gameObject);
        }
    }
}
