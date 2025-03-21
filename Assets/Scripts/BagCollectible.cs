using UnityEngine;

public class BagCollectible : DebtReducer
{
    [Header("Coin Settings")]
    [SerializeField] private float coinValue = 25f;

    private void Awake()
    {
        debtReductionAmount = coinValue;
    }

}
