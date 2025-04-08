public class DDSDifficultyEvent : Unity.Services.Analytics.Event
{
    public DDSDifficultyEvent() : base("dds_difficulty")
    {
    }

    // The average slot probability calculated by the DDS manager.
    public float avgSlotProbability { set { SetParameter("avgSlotProbability", value); } }
    // The classified difficulty level (e.g., "Easy", "Medium", "Hard").
    public string difficultyLevel { set { SetParameter("difficultyLevel", value); } }
    // The current debt ratio (currentDebt/maxDebt).
    public float debtRatio { set { SetParameter("debtRatio", value); } }
    // The player's money ratio relative to max debt.
    public float moneyRatio { set { SetParameter("moneyRatio", value); } }
}
