namespace Centurion.Accounts.App.Analytics.Model;

// activation
/*interval_start_timestamp: "2021-01-19T00:00:00+00:00"
new_members: 1
pct_communicated: 0
pct_opened_channels: 100*/
public class ActivationDiscordInsightsData : DiscordInsightsDataBase
{
  public int NewMembers { get; set; }
  public double PctCommunicated { get; set; }
  public double PctOpenedChannels { get; set; }
}