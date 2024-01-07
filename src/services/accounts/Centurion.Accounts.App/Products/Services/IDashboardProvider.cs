using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Services;

public interface IDashboardProvider
{
  ValueTask<DashboardData?> GetByOwnerIdAsync(long ownerId, CancellationToken ct = default);
  ValueTask<DashboardLoginData?> GetLoginDataAsync(IEnumerable<KeyValuePair<DashboardHostingMode, string>> modes,
    CancellationToken ct = default);
  ValueTask<ProductPublicInfoData?> GetPublicByDashboardIdAsync(Guid dashboardId, CancellationToken ct = default);
}