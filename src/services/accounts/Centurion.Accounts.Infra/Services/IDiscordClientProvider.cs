using Discord.WebSocket;

namespace Centurion.Accounts.Infra.Services;

public interface IDiscordClientProvider
{
  ValueTask<DiscordSocketClient> GetInitializedClientAsync(Guid dashboardId, CancellationToken ct = default);
}