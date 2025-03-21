using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class OutsideMapCheck : MonoBehaviour
{
    [SerializeField] private string gameOverScene = "GameOverScene"; // Set in Inspector
    [SerializeField] private float minY = -10f; // Minimum Y height before game over
    [SerializeField] private float maxY = 100f; // Maximum Y height before game over

    // Event raised when the player goes outside the allowed Y bounds.
    public static event Action OnOutsideMapGameOver;

    void Update()
    {
        // Check if player's Y position is out of bounds.
        if (transform.position.y < minY || transform.position.y > maxY)
        {
            // Raise the event so other scripts can react (e.g., log playtime, stop gameplay, etc.)
            OnOutsideMapGameOver?.Invoke();

            // Switch to the Game Over scene.
            SceneManager.LoadScene(gameOverScene);
        }
    }
}
