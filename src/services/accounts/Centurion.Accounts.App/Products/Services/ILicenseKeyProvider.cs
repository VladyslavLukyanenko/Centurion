using Centurion.Accounts.App.Products.Model;
using NodaTime;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Products.Services;

public interface ILicenseKeyProvider
{
  ValueTask<LicenseKeySummaryData> GetSummaryByIdAsync(Guid dashboardId, long id, CancellationToken ct = default);
  ValueTask<int> GetUsedTodayCountAsync(Guid dashboardId, Offset offset, CancellationToken ct = default);

  ValueTask<IPagedList<LicenseKeyShortData>> GetPageByReleaseIdAsync(Guid dashboardId, long releaseId,
    PageRequest pageRequest,
    CancellationToken ct = default);

  ValueTask<IPagedList<LicenseKeySnapshotData>> GetPageAsync(Guid dashboardId, LicenseKeyPageRequest pageRequest,
    CancellationToken ct = default);

  ValueTask<IList<PurchasedLicenseKeyData>> GetPurchasedKeysAsync(Guid dashboardId, long userId,
    CancellationToken ct = default);
}