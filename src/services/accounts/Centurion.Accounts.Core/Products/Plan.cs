using NodaTime;

namespace Centurion.Accounts.Core.Products;

public class Plan : DashboardBoundEntity
{
  private static readonly Duration DefaultLicenseLifetime = Duration.FromDays(30);
  private static readonly Duration DefaultUnbindableDelay = Duration.FromDays(0);
  private static readonly Duration DefaultTrialPeriodDuration = Duration.FromDays(30);

  public Plan(Guid dashboardId)
    : base(dashboardId)
  {
  }

  public bool IsUnbindable() => UnbindableDelay != null;

  public Instant? CalculateKeyExpiry() => IsTrial || LicenseLife == null
    ? null
    : SystemClock.Instance.GetCurrentInstant() + LicenseLife;

  public Instant CalculatePossibleKeyExpiry() =>
    SystemClock.Instance.GetCurrentInstant() + (LicenseLife ?? DefaultLicenseLifetime);

  public Instant? CalculateUnbindableAfter() => UnbindableDelay == null
    ? null
    : SystemClock.Instance.GetCurrentInstant() + UnbindableDelay;

  public Instant CalculatePossibleUnbindableAfter() =>
    SystemClock.Instance.GetCurrentInstant() + (UnbindableDelay ?? DefaultUnbindableDelay);

  public bool IsLifetimeLimited() => LicenseLife != null;

  public decimal Amount { get; set; }
  public string Currency { get; set; } = null!;
  public string? SubscriptionPlan { get; set; }
  public Duration? LicenseLife { get; set; }
  public Duration TrialPeriod { get; set; } = DefaultTrialPeriodDuration;
  public string Description { get; set; } = null!;
  public Duration? UnbindableDelay { get; set; }
  public bool IsTrial { get; set; }

  public ulong DiscordRoleId { get; set; }
  public bool ProtectPurchasesWithCaptcha { get; set; }

  public LicenseKeyGeneratorConfig LicenseKeyConfig { get; set; } = LicenseKeyGeneratorConfig.RandomString();
}