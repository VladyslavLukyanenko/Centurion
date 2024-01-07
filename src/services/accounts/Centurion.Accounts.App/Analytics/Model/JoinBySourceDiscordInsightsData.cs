namespace Centurion.Accounts.App.Analytics.Model;

// join-by-source {"interval_start_timestamp": "2021-01-19T00:00:00+00:00", "discovery_joins": 0, "invites": 1, "vanity_joins": 0}
public class JoinBySourceDiscordInsightsData : DiscordInsightsDataBase
{
  public int DiscoveryJoins { get; set; }
  public int Invites { get; set; }
  public int VanityJoins { get; set; }
}