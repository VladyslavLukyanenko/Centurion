using Centurion.Accounts.App.Model;

namespace Centurion.Accounts.App.Products.Services;

public class LicenseKeyPageRequest : FilteredPageRequest
{
  public bool? LifetimeOnly { get; set; }
  public long? PlanId { get; set; }
  public long? ReleaseId { get; set; }
  public LicensesSortBy? SortBy { get; set; }
}