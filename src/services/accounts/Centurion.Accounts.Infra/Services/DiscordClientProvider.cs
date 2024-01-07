using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.Infra.Services;

public class DiscordClientProvider : IDiscordClientProvider
{
  private static readonly ConcurrentDictionary<Guid, DiscordSocketClient> ClientsCache = new();
  private static readonly SemaphoreSlim Gates = new(1, 1);

  private readonly IDashboardRepository _dashboardRepository;

  public DiscordClientProvider(IDashboardRepository dashboardRepository)
  {
    _dashboardRepository = dashboardRepository;
  }

  public async ValueTask<DiscordSocketClient> GetInitializedClientAsync(Guid dashboardId,
    CancellationToken ct = default)
  {
    try
    {
      await Gates.WaitAsync(ct);
      if (!ClientsCache.TryGetValue(dashboardId, out var client))
      {
        client = new DiscordSocketClient();
        ClientsCache[dashboardId] = client;
      }

      if (client.LoginState != LoginState.LoggedIn)
      {
        var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId, ct);
        await client.LoginAsync(TokenType.Bot, dashboard!.DiscordConfig.BotAccessToken);
      }

      if (client.ConnectionState != ConnectionState.Connected)
      {
        await client.StartAsync();
      }

      return client;
    }
    finally
    {
      if (!ct.IsCancellationRequested)
      {
        Gates.Release();
      }
    }
  }
}