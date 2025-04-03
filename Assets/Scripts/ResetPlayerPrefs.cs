using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetPlayerPrefs : MonoBehaviour
{
    /// <summary>
    /// Resets all relevant PlayerPrefs keys used by the game.
    /// </summary>
    public void ResetAllPlayerPrefs()
    {
        ScoreManager.Instance.ResetAllPlayerPrefs();
    }
}
