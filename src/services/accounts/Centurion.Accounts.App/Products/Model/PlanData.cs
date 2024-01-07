namespace Centurion.Accounts.App.Products.Model;

public class PlanData
{
  public long Id { get; set; }
  public string Description { get; set; } = null!;
  public decimal Amount { get; set; }
  public string Currency { get; set; } = null!;

  public string? SubscriptionPlan { get; set; }
  public int? LicenseLifeDays { get; set; }
  public int? UnbindableDelayDays { get; set; }

  public bool IsTrial { get; set; }
  public int TrialPeriodDays { get; set; }

  public ulong DiscordRoleId { get; set; }
  public bool ProtectPurchasesWithCaptcha { get; set; }

  // public bool IsUnbindable { get; set; }
}