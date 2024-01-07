using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Services;

public interface IReleaseService
{
  ValueTask<long> CreateAsync(Guid dashboardId, SaveReleaseCommand cmd, CancellationToken ct = default);
  ValueTask UpdateAsync(Release release, SaveReleaseCommand cmd, CancellationToken ct = default);
}