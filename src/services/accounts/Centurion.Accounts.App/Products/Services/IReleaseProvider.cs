using Centurion.Accounts.App.Products.Model;
using CSharpFunctionalExtensions;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Products.Services;

public interface IReleaseProvider
{
  ValueTask<ReleaseData?> GetByIdAsync(Guid dashboardId, long id, CancellationToken ct = default);
  ValueTask<IList<ActiveReleaseInfoData>> GetActiveListAsync(Guid dashboardId, CancellationToken ct = default);

  ValueTask<IPagedList<ReleaseRowData>> GetPageAsync(Guid dashboardId, ReleasesPageRequest pageRequest,
    CancellationToken ct = default);

  ValueTask<Maybe<ReleaseStockData?>> GetStockByPasswordAsync(string password, CancellationToken ct = default);
}