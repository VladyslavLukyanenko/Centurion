using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.App.Products.Model;
using NodaTime;

namespace Centurion.Accounts.App.WebHooks.LicenseKeys;

public abstract class LicenseKeyAssociationChangeWebHookDataBase : WebHookDataBase
{
  public long Id { get; set; }
  public PlanRef Plan { get; set; } = null!;
  public UserRef? User { get; set; }
  public Instant? Expiry { get; set; }
  public string Value { get; set; } = null!;
}