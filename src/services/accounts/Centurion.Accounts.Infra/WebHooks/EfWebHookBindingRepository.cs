using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.WebHooks;
using Centurion.Accounts.Core.WebHooks.Services;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.WebHooks;

public class EfWebHookBindingRepository : EfCrudRepository<WebHookBinding>, IWebHookBindingRepository
{
  public EfWebHookBindingRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public ValueTask<WebHookBinding?> GetByTypeAsync(Guid dashboardId, string type, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public ValueTask<WebHooksConfig> GetConfigOfDashboardAsync(Guid dashboardId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }
}