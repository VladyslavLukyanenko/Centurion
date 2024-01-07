namespace Centurion.Accounts.App.Analytics.Model;

public class DiscordInsightsAnalytics
{
  public IList<JoinBySourceDiscordInsightsData> JoinBySource { get; set; } =
    new List<JoinBySourceDiscordInsightsData>();

  public IList<MembershipDiscordInsightsData> Membership { get; set; } = new List<MembershipDiscordInsightsData>();
  public IList<LeaversDiscordInsightsData> Leavers { get; set; } = new List<LeaversDiscordInsightsData>();
  public IList<ActivationDiscordInsightsData> Activation { get; set; } = new List<ActivationDiscordInsightsData>();
  public IList<RetentionDiscordInsightsData> Retention { get; set; } = new List<RetentionDiscordInsightsData>();
  public IList<OverviewDiscordInsightsData> Overview { get; set; } = new List<OverviewDiscordInsightsData>();
}