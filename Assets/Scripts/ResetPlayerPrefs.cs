using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetPlayerPrefs : MonoBehaviour
{
    /// <summary>
    /// Resets all relevant PlayerPrefs keys used by the game.
    /// </summary>
    public void ResetAllPlayerPrefs()
    {
        // MoneyManager keys
        PlayerPrefs.DeleteKey("CurrentMoney");
        PlayerPrefs.DeleteKey("CurrentDebt");
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.DeleteKey("TotalMoneyObtained");
        PlayerPrefs.DeleteKey("TotalDebtPaid");

        // LevelController key for playtime
        PlayerPrefs.DeleteKey("PlayTime");

        // ScoreManager keys for top-5 scores.
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey("DebtPaid_" + i);
            PlayerPrefs.DeleteKey("MoneyMade_" + i);
            PlayerPrefs.DeleteKey("PlayTime_" + i);
        }

        // Save the changes.
        PlayerPrefs.Save();

        Debug.Log("PlayerPrefs have been reset.");

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
