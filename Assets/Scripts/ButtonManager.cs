using TransitionsPlus;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    /// <summary>
    /// Loads the scene with the specified name.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    /// <summary>
    /// Restarts the game.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void RestartGame()
    {
        if (LevelController.Instance != null)
        {
            LevelController.Instance.ResetGame();
        }
        else
        {
            SceneManager.LoadScene("Start");
        }
    }
}
