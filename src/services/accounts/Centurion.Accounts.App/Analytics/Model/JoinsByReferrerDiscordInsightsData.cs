namespace Centurion.Accounts.App.Analytics.Model;

// joins-by-referrer
/*interval_start_timestamp: "2021-01-19T00:00:00+00:00"
joins: 1
referring_domain: "'dashboard.estocksoftware.com'"
----
interval_start_timestamp: "2021-01-18T00:00:00+00:00"
joins: 5
referring_domain: "'Unknown'"*/
public class JoinsByReferrerDiscordInsightsData : DiscordInsightsDataBase
{
  public string ReferringDomain { get; set; } = null!;
  public int Joins { get; set; }
}