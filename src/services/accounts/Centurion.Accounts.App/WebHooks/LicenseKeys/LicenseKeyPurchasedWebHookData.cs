using NodaTime;

namespace Centurion.Accounts.App.WebHooks.LicenseKeys;

[WebHookType("licensekeys.purchased")]
public class LicenseKeyPurchasedWebHookData : LicenseKeyAssociationChangeWebHookDataBase
{
  public bool IsTrial { get; set; }
  public Instant? TrialEndsAt { get; set; }
}