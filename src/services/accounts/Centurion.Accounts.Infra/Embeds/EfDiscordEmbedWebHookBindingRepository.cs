using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Embeds;
using Centurion.Accounts.Core.Embeds.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Embeds;

public class EfDiscordEmbedWebHookBindingRepository : EfCrudRepository<DiscordEmbedWebHookBinding>,
  IDiscordEmbedWebHookBindingRepository
{
  public EfDiscordEmbedWebHookBindingRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public ValueTask<DiscordEmbedWebHookBinding?> GetByEventTypeAsync(Guid dashboardId, string eventType,
    CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }
}