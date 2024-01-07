using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Centurion.Contracts.CloudManager;

namespace Centurion.TaskManager.Core.Services;

public class SingleNodeCloudManager : ICloudManager, ICloudClient, ICloudConnectionPool
{
  private readonly ConcurrentDictionary<string, Lazy<SingleNodeClientConnection>> _connections = new();
  private readonly CompositeDisposable _disposable = new();
  private readonly CloudServiceConfig _config;
  private readonly ILogger<SingleNodeCloudManager> _logger;

  public SingleNodeCloudManager(CloudServiceConfig config, ILogger<SingleNodeCloudManager> logger)
  {
    _config = config;
    _logger = logger;
  }

  public async Task EstablishConnection(Cloud.CloudClient client, CancellationToken stoppingToken)
  {
    stoppingToken.Register(_disposable.Clear);
    await Task.Delay(-1, stoppingToken);
  }

  public ICloudConnection? GetOrDefault(string userId) => _connections.GetOrDefault(userId)?.Value;
  public IEnumerable<ICloudConnection> Connections => _connections.Values.Select(_ => _.Value);

  public void KeepAlive(UserInfo userInfo)
  {
  }

  public IObservable<NodeStatus> Connect(UserInfo userInfo)
  {
    _connections.GetOrAdd(userInfo.UserId, static (uid, cfg) =>
      new Lazy<SingleNodeClientConnection>(() => new SingleNodeClientConnection(uid, cfg)), _config);

    _logger.LogDebug("User connected {UserName}#{UserId}", userInfo.UserName, userInfo.UserId);
    return Observable.Interval(TimeSpan.FromMilliseconds(500))
      .Select(_ => NodeStatus.Running)
      .Finally(() =>
      {
        _connections.Remove(userInfo.UserId, out _);
        _logger.LogDebug("User disconnected {UserName}#{UserId}", userInfo.UserName, userInfo.UserId);
      });
  }
}