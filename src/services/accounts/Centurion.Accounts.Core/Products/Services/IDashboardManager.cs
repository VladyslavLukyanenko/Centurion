namespace Centurion.Accounts.Core.Products.Services;

public interface IDashboardManager
{
  ValueTask<bool> TryJoinAsync(Guid dashboardId, long userId, CancellationToken ct = default);
}