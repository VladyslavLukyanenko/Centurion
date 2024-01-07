namespace Centurion.Accounts.App.Analytics.Model;

// joins-by-invite
/*interval_start_timestamp: "2021-01-19T00:00:00+00:00"
invite_link: "'https://discord.gg/Mmgy6SF'"
joins: 2*/
public class JoinsByInviteDiscordInsightsData : DiscordInsightsDataBase
{
  public string InviteLink { get; set; } = null!;
  public int Joins { get; set; }
}