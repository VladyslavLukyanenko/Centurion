using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.WebHooks.Services;

public interface IWebHookBindingRepository : ICrudRepository<WebHookBinding>
{
  ValueTask<WebHookBinding?> GetByTypeAsync(Guid dashboardId, string type, CancellationToken ct = default);
  ValueTask<WebHooksConfig> GetConfigOfDashboardAsync(Guid dashboardId, CancellationToken ct = default);
}