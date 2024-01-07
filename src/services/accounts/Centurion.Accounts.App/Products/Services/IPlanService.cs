using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Services;

public interface IPlanService
{
  ValueTask<long> CreateAsync(Guid dashboardId, PlanData data, CancellationToken ct = default);
  ValueTask UpdateAsync(Plan plan, PlanData data, CancellationToken ct = default);
}