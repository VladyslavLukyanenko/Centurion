namespace Centurion.Accounts.Core.Products.Events.LicenseKeys;

public class LicenseKeyBound : LicenseKeyAssociationChange
{
  public LicenseKeyBound(LicenseKey licenseKey, long userId, string? subscriptionId)
    : base(licenseKey, userId)
  {
    SubscriptionId = subscriptionId;
  }

  public string? SubscriptionId { get; }
}