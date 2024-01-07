using Centurion.Accounts.App.Model;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Services;

public class ReleasesPageRequest : FilteredPageRequest
{
  public long? PlanId { get; set; }
  public ReleaseType? Type { get; set; }
  public ReleasesSortBy? SortBy { get; set; }
}