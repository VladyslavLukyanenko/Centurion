namespace Centurion.Accounts.App.Analytics.Model;

// overview
/*interval_start_timestamp: "2021-01-19T00:00:00+00:00"
new_communicators: 0
new_members: 0
visitors: 40*/
public class OverviewDiscordInsightsData : DiscordInsightsDataBase
{
  public int NewCommunicators { get; set; }
  public int NewMembers { get; set; }
  public int Visitors { get; set; }
}