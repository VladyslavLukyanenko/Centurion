using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Contracts.CloudManager;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NodaTime;

namespace Centurion.TaskManager.Core.Services;

public class DistributedCloudManager : ICloudManager, ICloudClient, ICloudConnectionPool
{
  private record CloudCmdInfo(Instant Timestamp, KeepAliveCommand Command)
  {
    public CloudCmdInfo(KeepAliveCommand cmd) : this(SystemClock.Instance.GetCurrentInstant(), cmd)
    {
    }
  }

  private readonly ConcurrentDictionary<string, DistributedCloudConnection> _subscribers = new();
  private readonly Subject<CloudCmdInfo> _commands = new();

  private readonly ILogger<DistributedCloudManager> _logger;
  private readonly IObservable<IDictionary<string, KeepAliveCommand>> _incomingCommands;

  public DistributedCloudManager(ILogger<DistributedCloudManager> logger)
  {
    _logger = logger;
    _incomingCommands = _commands.AsObservable()
      .Buffer(TimeSpan.FromMilliseconds(300), ThreadPoolScheduler.Instance)
      .Where(b => b.Any())
      .Select(cmds =>
        cmds.GroupBy(_ => _.Command.UserId)
          .ToDictionary(_ => _.Key, _ => _.OrderByDescending(g => g.Timestamp).First().Command));
  }

  public async Task EstablishConnection(Cloud.CloudClient client, CancellationToken stoppingToken)
  {
    using var disposable = new CompositeDisposable();
    AsyncDuplexStreamingCall<CloudCommandBatch, NodeInfoBatch> conn = client.Connect(cancellationToken: stoppingToken);
    var destroy = new Subject<Unit>();
    _incomingCommands
      .TakeUntil(destroy)
      .Catch<IDictionary<string, KeepAliveCommand>, Exception>(exc =>
      {
        _logger.LogError(exc, "Error on writing keep-alive commands");
        return Observable.Empty<IDictionary<string, KeepAliveCommand>>();
      })
      .Synchronize()
      .Subscribe(cmds => conn.RequestStream.WriteAsync(new CloudCommandBatch
      {
        PerUserCommands = { cmds }
      }));

    disposable.Add(Disposable.Create(destroy, d =>
    {
      d.OnNext(Unit.Default);
      d.OnCompleted();
    }));
    stoppingToken.Register(disposable.Dispose);
    await foreach (var batch in conn.ResponseStream.ReadAllAsync(stoppingToken))
    {
      foreach (var (userId, info) in batch.PerUserInfo)
      {
        if (_subscribers.TryGetValue(userId, out var chan))
        {
          chan.OnNext(info);
        }
      }
    }
  }

  public void KeepAlive(UserInfo userInfo)
  {
    if (_subscribers.ContainsKey(userInfo.UserId))
    {
      _commands.OnNext(new CloudCmdInfo(new KeepAliveCommand
      {
        UserId = userInfo.UserId,
        UserName = userInfo.UserName,
        Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
      }));
    }
  }

  public IObservable<NodeStatus> Connect(UserInfo userInfo)
  {
    var statusChanges = _subscribers.GetOrAdd(userInfo.UserId, uid => new DistributedCloudConnection(uid))
      .AsObservable()
      .Select(_ => _.Status)
      .Finally(() =>
      {
        _subscribers.Remove(userInfo.UserId, out var conn);
        conn?.Dispose();
      });

    return statusChanges;
  }

  public ICloudConnection? GetOrDefault(string userId)
  {
    if (!_subscribers.TryGetValue(userId, out var infoSubj) || infoSubj.IsEmpty)
    {
      return null;
    }

    return infoSubj;
  }

  public IEnumerable<ICloudConnection> Connections => _subscribers.Values.Where(_ => !_.IsEmpty);
}

public class DistributedCloudConnection : ISubject<NodeInfo>, ICloudConnection
{
  private readonly CompositeDisposable _disposable = new();
  private readonly BehaviorSubject<NodeInfo?> _nodeInfo = new(null);

  public DistributedCloudConnection(string userId)
  {
    UserId = userId;
    Id = Guid.NewGuid().ToString();
    ConnectedAt = SystemClock.Instance.GetCurrentInstant();

    _disposable.Add(Disposable.Create(this, static self =>
    {
      self._nodeInfo.OnCompleted();
      self.Closed?.Invoke(self, EventArgs.Empty);
    }));
    _disposable.Add(_nodeInfo);
  }

  public string Id { get; }
  public Instant ConnectedAt { get; }
  public string? DnsName => _nodeInfo.Value?.DnsName;
  public string UserId { get; }
  public event EventHandler? Closed;

  public bool IsEmpty => string.IsNullOrEmpty(DnsName);

  public void OnCompleted()
  {
    _disposable.Clear();
  }

  public void OnError(Exception error)
  {
    _nodeInfo.OnError(error);
  }

  public void OnNext(NodeInfo value)
  {
    _nodeInfo.OnNext(value);
  }

  public IDisposable Subscribe(IObserver<NodeInfo> observer)
  {
    var sub = _nodeInfo.Where(it => it != null).Subscribe(observer!);
    _disposable.Add(sub);
    return sub;
  }

  public void Dispose()
  {
    _disposable.Dispose();
  }
}