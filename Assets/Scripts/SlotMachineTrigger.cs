using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlotMachineTrigger : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (MoneyManager.Instance.GetMoney() > 0)
                SlotMachineOverlay.Instance.ShowSlotMachine();
            else
                SlotMachineOverlay.Instance.DisplayNoMoneyMessage();
        }
    }
}
