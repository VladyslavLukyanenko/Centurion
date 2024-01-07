namespace Centurion.Accounts.Core.Products.Events.LicenseKeys;

public class LicenseKeyRemoved : LicenseKeyAssociationChange
{
  public LicenseKeyRemoved(LicenseKey licenseKey)
    : base(licenseKey, licenseKey.UserId)
  {
  }
}