namespace Centurion.Accounts.App.Analytics.Model;

// membership {"interval_start_timestamp": "2021-01-20T00:00:00+00:00", "total_membership": 3122}
public class MembershipDiscordInsightsData : DiscordInsightsDataBase
{
  public int TotalMembership { get; set; }
}