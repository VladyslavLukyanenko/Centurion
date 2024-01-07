namespace Centurion.Accounts.App.Analytics.Model;

// leavers {"interval_start_timestamp": "2021-01-20T00:00:00+00:00", "days_in_guild": "'Members for 1 month+'", "leavers": 9}
public class LeaversDiscordInsightsData : DiscordInsightsDataBase
{
  public string DaysInGuild { get; set; } = null!;
  public int Leavers { get; set; }
}