using UnityEngine;

public class EuroCollectible : DebtReducer
{
    [Header("Coin Settings")]
    [SerializeField] private float coinValue = 5f;

    [Header("Particle Settings")]
    [Tooltip("Drag in the ParticleSystem that should play when this collectible is triggered.")]
    [SerializeField] private ParticleSystem collectParticles;

    // Store the original local scale of the particle system.
    private Vector3 originalParticleScale;

    private void Awake()
    {
        debtAmount = coinValue;

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
            // Now reduce debt (and destroy this collectible).
            ReduceDebt();
        }
    }
}
