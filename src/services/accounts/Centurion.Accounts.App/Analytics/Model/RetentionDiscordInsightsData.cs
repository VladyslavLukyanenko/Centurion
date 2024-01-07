namespace Centurion.Accounts.App.Analytics.Model;

// retention
/*interval_start_timestamp: "2020-12-22T00:00:00+00:00"
new_members: 1
pct_retained: 100*/
public class RetentionDiscordInsightsData : DiscordInsightsDataBase
{
  public int NewMembers { get; set; }
  public double PctRetained { get; set; }
}