using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Centurion.Contracts;
using Centurion.Contracts.Monitor.Integration;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Centurion.Monitor.App.Sites.FakeShop;

[Monitor(SupportedSites.FakeShop)]
public class FakeShopMonitor : IStoreMonitor
{
#if DEBUG
  private const string ProductUrl = "https://localhost:5100/fakeproduct";
#else
  private const string ProductUrl = "https://fakeshop.centurion.gg/fakeproduct";
#endif

  private readonly IServiceScope _serviceScope;
  private bool _isDisposed;
  private IMonitorHttpClientFactory _clientFactory = null!;

  public FakeShopMonitor(IServiceScopeFactory serviceScopeFactory)
  {
    _serviceScope = serviceScopeFactory.CreateScope();
  }

  public ValueTask DisposeAsync()
  {
    if (!_isDisposed)
    {
      _serviceScope.Dispose();
      _isDisposed = true;
    }

    return default;
  }

  public bool IsInitialized { get; private set; }

#pragma warning disable 1998
  public async IAsyncEnumerable<MonitoringStatusChanged> Initialize(MonitorTarget target,
#pragma warning restore 1998
    [EnumeratorCancellation] CancellationToken ct)
  {
    _clientFactory = new MonitorHttpClientFactory(target.Module, _serviceScope.ServiceProvider,
      target.Settings.AntibotConfig, target.Settings.Proxies);

    IsInitialized = true;

    yield break;
  }

  public async IAsyncEnumerable<MonitoringStatusChanged> Monitor(MonitorTarget target,
    [EnumeratorCancellation] CancellationToken ct)
  {
    while (!ct.IsCancellationRequested)
    {
      var client = _clientFactory.CreateHttpClient();
      var request = new HttpRequestMessage(HttpMethod.Get, ProductUrl);
      var response = await client.SendAsync(request, ct);
      var data = await response.Content.ReadFromJsonAsync<FakeShopResponse>(cancellationToken: ct);
      if (data!.IsAvailable)
      {
        yield return MonitoringStatusChanged.InStock(target);
        break;
      }

      yield return MonitoringStatusChanged.OutOfStock(target);
    }
  }
}