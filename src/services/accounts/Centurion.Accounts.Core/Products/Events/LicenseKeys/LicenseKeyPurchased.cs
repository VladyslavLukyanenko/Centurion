using NodaTime;

namespace Centurion.Accounts.Core.Products.Events.LicenseKeys;

public class LicenseKeyPurchased : LicenseKeyAssociationChange
{
  public LicenseKeyPurchased(LicenseKey licenseKey, long userId, Instant? trialEndsAt)
    : base(licenseKey, userId)
  {
    TrialEndsAt = trialEndsAt;
  }

  public Instant? TrialEndsAt { get; }
}