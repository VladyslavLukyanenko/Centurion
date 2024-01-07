using Centurion.Accounts.App.Identity.Model;
using NodaTime;

namespace Centurion.Accounts.App.Products.Model;

public class LicenseKeySummaryData
{
  public long Id { get; set; }
    
  public KeyOwnerSummaryData? User { get; set; }
  public long PlanId { get; set; }

  public Instant? Expiry { get; set; }
    
  public string Value { get; set; } = null!;
  public string? Reason { get; set; }

  public Instant? TrialEndsAt { get; set; }
  public Instant? UnbindableAfter { get; set; }

  public class KeyOwnerSummaryData : UserRef
  {
    public Instant? JoinedAt { get; set; }
  }
}