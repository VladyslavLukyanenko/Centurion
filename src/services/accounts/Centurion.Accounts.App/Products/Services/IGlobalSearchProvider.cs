using Centurion.Accounts.App.Products.Collections;

namespace Centurion.Accounts.App.Products.Services;

public interface IGlobalSearchProvider
{
  ValueTask<IGlobalSearchPagedList> GetAllAsync(Guid dashboardId, GlobalSearchPageRequest pageRequest,
    CancellationToken ct = default);
}