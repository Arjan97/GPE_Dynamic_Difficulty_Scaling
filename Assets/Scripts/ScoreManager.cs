using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private float scorePerSecond = 1f;
    [SerializeField] private TMP_Text scoreText; // or TextMeshProUGUI scoreText;

    private float score = 0f;
    private const string ScoreKey = "Score";

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load the saved score (default is 0 if not found)
        score = PlayerPrefs.GetFloat(ScoreKey, 0f);
        UpdateScoreUI();
    }

    private void Update()
    {
        // Increase score over time.
        AddScore(scorePerSecond * Time.deltaTime);
    }

    /// <summary>
    /// Public method to add to the score. For example, call this from DebtReducer.ReduceDebt.
    /// </summary>
    public void AddScore(float amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    /// <summary>
    /// Updates the score text UI, if assigned.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // You can format the score as needed.
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
        }
    }

    /// <summary>
    /// Save the current score to PlayerPrefs.
    /// </summary>
    private void SaveScore()
    {
        PlayerPrefs.SetFloat(ScoreKey, score);
        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SaveScore();
    }

    private void OnDestroy()
    {
        SaveScore();
    }

    /// <summary>
    /// Optional getter for the current score.
    /// </summary>
    public float GetScore()
    {
        return score;
    }
}
