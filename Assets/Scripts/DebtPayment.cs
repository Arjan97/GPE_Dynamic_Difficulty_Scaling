using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class DebtPayment : MonoBehaviour
{
    [Header("Particle Settings")]
    [Tooltip("Particle system to play when this trigger is activated.")]
    [SerializeField] private ParticleSystem collectParticles;
    private Vector3 originalParticleScale;

    private void Awake()
    {
        if (collectParticles != null)
        {
            originalParticleScale = collectParticles.transform.localScale;
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
                // Detach and play particle effect.
                //collectParticles.transform.parent = null;
                collectParticles.transform.localScale = originalParticleScale;
                collectParticles.gameObject.SetActive(true);
                collectParticles.Play();
            }

            // Activate the DebtPaymentOverlay.
            if (DebtPaymentOverlay.Instance != null)
            {
                if (MoneyManager.Instance.GetMoney() > 0)
                    DebtPaymentOverlay.Instance.ShowOverlay();
                else
                    SlotMachineOverlay.Instance.DisplayNoMoneyMessage();
            }

            // Destroy this trigger so it doesn't trigger again.
            Destroy(gameObject);
        }
    }
}
