using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Products.Services;

public interface IUserProfileService
{
  ValueTask RefreshUserProfileIfOutdatedAsync(long userId, Guid dashboardId, CancellationToken ct = default);
  ValueTask RefreshUserProfileIfOutdatedAsync(long userId, Dashboard dashboard, CancellationToken ct = default);
}