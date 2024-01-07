using NodaTime;

namespace Centurion.Accounts.App.Products.Model;

public class PurchasedLicenseKeyData : LicenseKeySnapshotData
{
  public bool HasActiveSession { get; set; }
  public bool IsUnbindable { get; set; }
  public Instant? LastAuthRequest { get; set; }
  public bool IsSubscriptionCancelled { get; set; }
  public bool IsExpired { get; set; }
}