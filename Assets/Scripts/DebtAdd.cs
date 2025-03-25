using UnityEngine;

public class DebtAdd : DebtReducer
{
    [Header("Debt Addition Settings")]
    [SerializeField] private float debtToAdd = 10f;
    [Header("Particle Settings")]
    [Tooltip("Drag in the ParticleSystem that should play when this collectible is triggered.")]
    [SerializeField] private ParticleSystem collectParticles;

    // Store the original local scale of the particle system.
    private Vector3 originalParticleScale;
    private void Awake()
    {
        // Use debtToAdd as the debt amount for this pickup,
        // and set the flag so that it adds debt.
        debtAmount = debtToAdd;
        isDebtAddition = true;

        if (collectParticles != null)
        {
            // Cache the original scale from the prefab.
            originalParticleScale = collectParticles.transform.localScale;

            // Ensure the particle system is not playing initially.
            collectParticles.Stop();
            collectParticles.gameObject.SetActive(false);
        }
    }
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectParticles != null)
            {
                // Detach the particle system so it persists after this object is destroyed.
                collectParticles.transform.parent = null;

                // Reapply the original scale to avoid inheriting any parent's scale.
                collectParticles.transform.localScale = originalParticleScale;

                collectParticles.gameObject.SetActive(true);
                collectParticles.Play();
            }
            // Now add debt (and destroy this collectible).
            AddDebt();
        }
    }
}
