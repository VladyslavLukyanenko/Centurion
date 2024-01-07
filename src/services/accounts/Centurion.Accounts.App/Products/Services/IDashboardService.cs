using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Services;

public interface IDashboardService
{
  ValueTask UpdateAsync(Dashboard dashboard, DashboardData cmd, CancellationToken ct = default);
}