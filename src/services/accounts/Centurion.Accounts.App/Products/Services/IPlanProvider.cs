using Centurion.Accounts.App.Products.Model;

namespace Centurion.Accounts.App.Products.Services;

public interface IPlanProvider
{
  ValueTask<IList<PlanData>> GetAllAsync(Guid dashboardId, CancellationToken ct = default);
  ValueTask<PlanData?> GetByIdAsync(long planId, CancellationToken ct = default);
}