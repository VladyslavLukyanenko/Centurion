using Centurion.Accounts.App.Products.Model;

namespace Centurion.Accounts.App.Products.Services;

public interface IDashboardRefProvider
{
  ValueTask<DashboardRef> GetRefAsync(Guid dashboardId, CancellationToken ct = default);

  ValueTask<IDictionary<Guid, DashboardRef>> GetRefsAsync(IEnumerable<Guid> dashboardIds,
    CancellationToken ct = default);
}