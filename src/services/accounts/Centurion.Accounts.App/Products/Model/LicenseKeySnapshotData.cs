using NodaTime;

namespace Centurion.Accounts.App.Products.Model;

public class LicenseKeySnapshotData : LicenseKeyShortData
{
  public Instant? Expiry { get; set; }
  public string? ReleaseTitle { get; set; }
  public string PlanDesc { get; set; } = null!;
}