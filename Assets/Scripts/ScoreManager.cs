using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private const int maxScores = 5;

    // Separate lists for each category.
    private List<float> debtPaidScores = new List<float>();
    private List<float> moneyMadeScores = new List<float>();
    private List<float> playTimeScores = new List<float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDebtPaidScore(float score)
    {
        debtPaidScores.Add(score);
        debtPaidScores = debtPaidScores.OrderByDescending(s => s).Take(maxScores).ToList();
        SaveScores("DebtPaid", debtPaidScores);
    }

    public void AddMoneyMadeScore(float score)
    {
        moneyMadeScores.Add(score);
        moneyMadeScores = moneyMadeScores.OrderByDescending(s => s).Take(maxScores).ToList();
        SaveScores("MoneyMade", moneyMadeScores);
    }

    public void AddPlayTimeScore(float score)
    {
        playTimeScores.Add(score);
        playTimeScores = playTimeScores.OrderByDescending(s => s).Take(maxScores).ToList();
        SaveScores("PlayTime", playTimeScores);
    }

    private void SaveScores(string keyPrefix, List<float> scores)
    {
        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetFloat(keyPrefix + "_" + (i + 1), scores[i]);
        }
        // Clear any old entries beyond maxScores.
        for (int i = scores.Count; i < maxScores; i++)
        {
            PlayerPrefs.DeleteKey(keyPrefix + "_" + (i + 1));
        }
        PlayerPrefs.Save();
    }

    private void LoadScores()
    {
        // Load debt paid
        debtPaidScores.Clear();
        for (int i = 0; i < maxScores; i++)
        {
            string key = "DebtPaid_" + (i + 1);
            if (PlayerPrefs.HasKey(key))
                debtPaidScores.Add(PlayerPrefs.GetFloat(key));
        }
        // Load money made
        moneyMadeScores.Clear();
        for (int i = 0; i < maxScores; i++)
        {
            string key = "MoneyMade_" + (i + 1);
            if (PlayerPrefs.HasKey(key))
                moneyMadeScores.Add(PlayerPrefs.GetFloat(key));
        }
        // Load play time
        playTimeScores.Clear();
        for (int i = 0; i < maxScores; i++)
        {
            string key = "PlayTime_" + (i + 1);
            if (PlayerPrefs.HasKey(key))
                playTimeScores.Add(PlayerPrefs.GetFloat(key));
        }
    }

    // Public getters
    public List<float> GetDebtPaidScores() => new List<float>(debtPaidScores);
    public List<float> GetMoneyMadeScores() => new List<float>(moneyMadeScores);
    public List<float> GetPlayTimeScores() => new List<float>(playTimeScores);
}
