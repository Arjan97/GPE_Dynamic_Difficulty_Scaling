using UnityEngine;
using UnityEngine.SceneManagement;

public class OutsideMapCheck : MonoBehaviour
{
    [SerializeField] string gameOverScene = "GameOverScene"; // Set in Inspector
    [SerializeField] float minY = -10f; // Minimum Y height before respawn
    [SerializeField] float maxY = 100f; // Maximum Y height before respawn

    void Update()
    {
        // Check if player's Y position is out of bounds
        if (transform.position.y < minY || transform.position.y > maxY)
        {
            SceneManager.LoadScene(gameOverScene); // Load the Game Over scene
        }
    }
}
