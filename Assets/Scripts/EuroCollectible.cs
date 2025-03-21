using UnityEngine;

public class EuroCollectible : DebtReducer
{
    [Header("Coin Settings")]
    [SerializeField] private float coinValue = 5f;

    private void Awake()
    {
        debtReductionAmount = coinValue;
    }

}
