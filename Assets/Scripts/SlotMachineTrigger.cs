using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlotMachineTrigger : MonoBehaviour
{
    [Header("Particle Settings")]
    [Tooltip("Drag in the ParticleSystem that should play when this collectible is triggered.")]
    [SerializeField] private ParticleSystem collectParticles;

    // Store the original local scale of the particle system.
    private Vector3 originalParticleScale;
    private SlotMachineOverlay slotMachineOverlay;
    private void Awake()
    {
        if (slotMachineOverlay == null)
            slotMachineOverlay = FindFirstObjectByType<SlotMachineOverlay>();
        if (collectParticles != null)
        {
            // Cache the original scale from the prefab.
            originalParticleScale = collectParticles.transform.localScale;

            // Ensure the particle system is not playing initially.
            collectParticles.Stop();
            collectParticles.gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
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
            if (MoneyManager.Instance.GetMoney() > 0)
                slotMachineOverlay.ShowSlotMachine();
            else
                slotMachineOverlay.DisplayNoMoneyMessage();
        }
    }
}
