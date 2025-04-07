using UnityEngine;
using System.Collections;

public class ChaseScript : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public InfiniteRunnerMovement playerMovement;

    [Header("Behind Distance (Local Z)")]
    public float behindDistance = -10f;
    public float nearDistance = 0f;

    [Header("Lateral Offset Settings (X or Z)")]
    public float rightOffset = -1.6f;
    public float leftOffset = 1.6f;
    public float yOffset = 0f;
    public float lateralLerpSpeed = 5f;

    [Header("Movement Smoothing")]
    public float moveLerpSpeed = 5f;
    public float rotationLerpSpeed = 5f;

    [Header("Grab Settings")]
    public float grabAnimationDuration = 1f;
    public Animator animator;

    [Header("Audio Settings")]
    [Tooltip("Voice line played at the start of the chase.")]
    public AudioClip startVoiceLine;
    [Tooltip("Voice line played when chaser is halfway to the player.")]
    public AudioClip halfWayVoiceLine;
    [Tooltip("Voice line played when chaser is three-quarters close to the player.")]
    public AudioClip threeQuarterVoiceLine;
    [Tooltip("Voice line played when the chaser grabs the player.")]
    public AudioClip grabVoiceLine;

    private AudioSource audioSource;
    private bool isGrabbed = false;
    private float currentXOffset = 0f;

    // ADDED: Variables for replay delay (10 seconds)
    private float lastHalfVoiceTime = 0f;
    private float lastThreeQuarterVoiceTime = 0f;
    private bool playedStartSound = false;
    private void Start()
    {
        playedStartSound = false;
        if (player == null || playerMovement == null)
        {
            Debug.LogError("ChaserScript: Missing player or movement script reference.");
            enabled = false;
            return;
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (isGrabbed) return;  // If we've grabbed the player, stop updating

        if (MoneyManager.Instance == null) return;

        float currentDebt = MoneyManager.Instance.GetDebt();
        float maxDebt = MoneyManager.Instance.GetMaxDebt();
        float debtRatio = Mathf.Clamp01(currentDebt / maxDebt);

        // Interpolate behindDistance -> nearDistance
        float localZ = Mathf.Lerp(behindDistance, nearDistance, debtRatio);

        float h = playerMovement.HorizontalInput;
        float desiredX = 0f;
        if (h > 0.1f) desiredX = leftOffset;
        else if (h < -0.1f) desiredX = rightOffset;
        else desiredX = 0f;

        currentXOffset = Mathf.Lerp(currentXOffset, desiredX, Time.deltaTime * lateralLerpSpeed);

        Vector3 targetLocalPos = new Vector3(currentXOffset, yOffset, localZ);
        Vector3 targetWorldPos = player.TransformPoint(targetLocalPos);

        // Move & rotate
        transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * moveLerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, player.rotation, Time.deltaTime * rotationLerpSpeed);

        // Compute normalized distance (0 when at behindDistance, 1 when at nearDistance)
        float normalizedDistance = (localZ - behindDistance) / (nearDistance - behindDistance);


        // Play start voice line once at the beginning
        if (normalizedDistance >= 0.3f && startVoiceLine != null && !audioSource.isPlaying && !playedStartSound)
            {
                audioSource.PlayOneShot(startVoiceLine);
            playedStartSound = true;
            }
    
        //  Trigger half-way voice line if normalizedDistance >= 0.5 and 10 seconds have passed
        if (normalizedDistance >= 0.5f && Time.time - lastHalfVoiceTime >= 10f && !audioSource.isPlaying)
        {
            if (halfWayVoiceLine != null)
            {
                audioSource.PlayOneShot(halfWayVoiceLine);
            }
            lastHalfVoiceTime = Time.time;
        }

        //  Trigger three-quarter voice line if normalizedDistance >= 0.75 and 10 seconds have passed
        if (normalizedDistance >= 0.75f && Time.time - lastThreeQuarterVoiceTime >= 10f && !audioSource.isPlaying)
        {
            if (threeQuarterVoiceLine != null)
            {
                audioSource.PlayOneShot(threeQuarterVoiceLine);
            }
            lastThreeQuarterVoiceTime = Time.time;
        }
    }

    #region Mimicking Jump / Fall

    /// <summary>
    /// Called by player script to mimic a jump on the chaser.
    /// </summary>
    public void Jump()
    {
        if (animator != null)
        {
            animator.SetTrigger("hasJumped");
        }
    }

    /// <summary>
    /// Called by player script to set the chaser's isFalling bool.
    /// </summary>
    public void SetFalling(bool falling)
    {
        if (animator != null)
        {
            animator.SetBool("isFalling", falling);
        }
    }

    #endregion

    #region Grab & Game Over

    private void OnEnable()
    {
        MoneyManager.OnGameOver += TriggerGrab;
    }

    private void OnDisable()
    {
        if (MoneyManager.Instance != null)
            MoneyManager.OnGameOver -= TriggerGrab;
    }

    private void TriggerGrab()
    {
        isGrabbed = true;
        if (animator != null)
        {
            animator.SetTrigger("grabPlayer");
        }
        // Player's game over animation
        GameObject playerAnimObj = GameObject.FindGameObjectWithTag("PlayerAnim");
        if (playerAnimObj != null)
        {
            Animator playerAnim = playerAnimObj.GetComponent<Animator>();
            playerAnim.SetTrigger("gameOver");
        }
        // Play grab voice line
        if (grabVoiceLine != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(grabVoiceLine);
        }
        StartCoroutine(HandleGrabAndGameOver());
    }

    private IEnumerator HandleGrabAndGameOver()
    {
        yield return new WaitForSeconds(grabAnimationDuration);
        LevelController.Instance.HandleGameOver();
    }

    #endregion
}
