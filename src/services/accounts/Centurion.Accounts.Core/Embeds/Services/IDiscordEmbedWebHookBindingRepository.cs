using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Embeds.Services;

public interface IDiscordEmbedWebHookBindingRepository : ICrudRepository<DiscordEmbedWebHookBinding>
{
  ValueTask<DiscordEmbedWebHookBinding?> GetByEventTypeAsync(Guid dashboardId, string eventType,
    CancellationToken ct = default);
}