using Unity.Services.Analytics;

public class SessionEndEvent : Unity.Services.Analytics.Event
{
    public SessionEndEvent() : base("session_end")
    {
    }

    // Total playtime for the session (in seconds).
    public float total_sessiontime { set { SetParameter("total_sessiontime", value); } }

    public string username { set { SetParameter("username", value); } }
    // Number of times the player replayed the game.
    public int replay_count { set { SetParameter("replay_count", value); } }
    // Total money made during the session.
    public float money_made { set { SetParameter("money_made", value); } }
    // Total debt paid during the session.
    public float debt_paid { set { SetParameter("debt_paid", value); } }
    // Amount of money lost during the session.
    public float money_lost { set { SetParameter("money_lost", value); } }
    // Whether Dynamic Difficulty Scaling was enabled during the session.
    public bool dds_enabled { set { SetParameter("dds_enabled", value); } }
}
