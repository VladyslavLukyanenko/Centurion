namespace Centurion.Accounts.Core.Products.Events.LicenseKeys;

public class LicenseKeyUnbound : LicenseKeyAssociationChange
{
  public LicenseKeyUnbound(LicenseKey licenseKey, string? subscriptionId, string? sessionId, long userId,
    string previousValue)
    : base(licenseKey, userId)
  {
    PreviousSubscriptionId = subscriptionId;
    PreviousSessionId = sessionId;
    PreviousUserId = userId;
    PreviousValue = previousValue;
  }

  public string? PreviousSubscriptionId { get; }
  public string? PreviousSessionId { get; }
  public long PreviousUserId { get; }
  public string PreviousValue { get; }
}