using Centurion.Accounts.Core.Events;
using NodaTime;

namespace Centurion.Accounts.Core.Products.Events.LicenseKeys;

public abstract class LicenseKeyAssociationChange : DomainEvent, IDashboardBoundEvent
{
  protected LicenseKeyAssociationChange(LicenseKey licenseKey, long? userId)
  {
    LicenseKeyId = licenseKey.Id;
    PlanId = licenseKey.PlanId;
    UserId = userId;
    DashboardId = licenseKey.DashboardId;
    Expiry = licenseKey.Expiry;
    Value = licenseKey.Value;
  }

  public long LicenseKeyId { get; }
  public long PlanId { get; }
  public long? UserId { get; }
  public Guid DashboardId { get; }
  public Instant? Expiry { get; }
  public string Value { get; }
}