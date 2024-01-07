using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using Centurion.SeedWork;
using Centurion.SeedWork.Primitives;
using Docker.DotNet;
using Polly;

namespace Centurion.CloudManager.Web.Services;

public class TokenBucketExecutionScheduler : BackgroundService, IExecutionScheduler
{
  private const int DockerApiCallMaxAttemptCount = 10;
  private static readonly object Stub = new();
  private readonly ConcurrentDictionary<string, object> _scheduledContainerStops = new();
  private readonly ConcurrentDictionary<string, object> _scheduledContainerStarts = new();

  private readonly IThrottlingQueue<string> _startNodesQueue;
  private readonly IThrottlingQueue<string> _shutdownNodesQueue;

  private readonly Subject<string> _startImagesNodeStream = new();
  private readonly Subject<string> _stopImagesNodeStream = new();

  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<TokenBucketExecutionScheduler> _logger;


  private readonly AsyncPolicy _dockerApiPolicy;

  public TokenBucketExecutionScheduler(IServiceProvider serviceProvider, ILogger<TokenBucketExecutionScheduler> logger,
    ILoggerFactory loggerFactory)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;

    var startQueueLogger = loggerFactory.CreateLogger("StartNodeQueue");
    var shutdownQueueLogger = loggerFactory.CreateLogger("ShutdownNodeQueue");

    _startNodesQueue = new TokenBucketThrottlingQueue<string>(2, 100, logger: startQueueLogger);
    _shutdownNodesQueue = new TokenBucketThrottlingQueue<string>(2, 100, logger: shutdownQueueLogger);

    _dockerApiPolicy = Policy.HandleInner<SocketException>()
      .OrInner<DockerApiException>()
      .WaitAndRetryAsync(DockerApiCallMaxAttemptCount,
        att => att.ToJitteredDelayTime(),
        (exc, delay, retryAttempt, _) =>
          logger.LogError("Failed to call docker API. Attempt {RetryAttempt} in {RetryDelay}. Cause: {Cause}", retryAttempt,
            delay, exc.GetBaseException().Message)
      );
  }

  public void ScheduleStartNewNodes(params string[] userIds)
  {
    var scheduledNodeStarts = _startNodesQueue.SubmitUniqueRange(userIds);
    if (scheduledNodeStarts.Any())
    {
      _logger.LogDebug("Scheduled node spawning for {@UserIds}", scheduledNodeStarts);
    }
  }

  public void ScheduleShutdown(string nodeId)
  {
    if (_shutdownNodesQueue.SubmitUniqueRange(nodeId).Any())
    {
      _logger.LogDebug("Scheduled shutting-down of {NodeId}", nodeId);
    }
  }

  public void ScheduleStartContainers(string nodeId)
  {
    if (_scheduledContainerStarts.ContainsKey(nodeId))
    {
      return;
    }

    _scheduledContainerStarts[nodeId] = Stub;
    _startImagesNodeStream.OnNext(nodeId);
    _logger.LogDebug("Scheduled container start for {NodeId}", nodeId);
  }

  public void ScheduleStopContainers(string nodeId)
  {
    if (_scheduledContainerStops.ContainsKey(nodeId))
    {
      return;
    }

    _scheduledContainerStops[nodeId] = Stub;
    _stopImagesNodeStream.OnNext(nodeId);
    _logger.LogDebug("Scheduled containers stopping for {NodeId}", nodeId);
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        ProcessRequests(stoppingToken);
        await Task.Delay(-1, stoppingToken);
      }
      catch (OperationCanceledException exc) when (exc.CancellationToken.IsCancellationRequested)
      {
        break;
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Error on processing scheduled requests");
      }
    }
  }

  private void ProcessRequests(CancellationToken stoppingToken)
  {
    var lifetime = new CompositeDisposable();
    stoppingToken.Register(lifetime.Dispose);
    _startNodesQueue
      .Select(ids => ids as string[] ?? ids.ToArray())
      .ObserveOn(ThreadPoolScheduler.Instance)
      .Select(ids => Observable.FromAsync(async () =>
      {
        using var scope = _serviceProvider.CreateScope();
        var infra = scope.ServiceProvider.GetRequiredService<IInfrastructureClient>();
        var repo = scope.ServiceProvider.GetRequiredService<INodeSnapshotRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await StartNewNodes(ids, repo, infra, uow, stoppingToken);
      }))
      .Switch()
      .Subscribe()
      .DisposeWith(lifetime);

    _shutdownNodesQueue
      .Select(ids => ids as string[] ?? ids.ToArray())
      .ObserveOn(ThreadPoolScheduler.Instance)
      .Select(ids => Observable.FromAsync(async () =>
      {
        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IInfrastructureClient>();

        await ShutdownNodes(client, ids, stoppingToken);
      }))
      .Switch()
      .Subscribe()
      .DisposeWith(lifetime);

    _startImagesNodeStream
      .Buffer(TimeSpan.FromMilliseconds(200), ThreadPoolScheduler.Instance)
      .Where(b => b.Any())
      .Select(ids => ids as string[] ?? ids.ToArray())
      .Select(ids => Observable.FromAsync(async () =>
      {
        using var scope = _serviceProvider.CreateScope();
        var imagesManager = scope.ServiceProvider.GetRequiredService<IImagesManager>();
        var cfg = scope.ServiceProvider.GetRequiredService<CheckoutServiceSpawnConfig>();
        var imagesRepo = scope.ServiceProvider.GetRequiredService<IImageInfoRepository>();
        var client = scope.ServiceProvider.GetRequiredService<IInfrastructureClient>();

        await StartContainers(ids, cfg, imagesRepo, client, imagesManager, stoppingToken);
      }))
      .Switch()
      .Subscribe()
      .DisposeWith(lifetime);

    _stopImagesNodeStream
      .Buffer(TimeSpan.FromMilliseconds(200), ThreadPoolScheduler.Instance)
      .Where(b => b.Any())
      .Select(ids => ids as string[] ?? ids.ToArray())
      .Select(ids => Observable.FromAsync(async () =>
      {
        using var scope = _serviceProvider.CreateScope();
        var imagesManager = scope.ServiceProvider.GetRequiredService<IImagesManager>();
        var client = scope.ServiceProvider.GetRequiredService<IInfrastructureClient>();
        var repo = scope.ServiceProvider.GetRequiredService<INodeSnapshotRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await CleanupAndStopContainers(ids, client, repo, uow, imagesManager, stoppingToken);
      }))
      .Switch()
      .Subscribe()
      .DisposeWith(lifetime);

    _ = Task.Factory.StartNew(() => _startNodesQueue.ProcessBlocking(stoppingToken), stoppingToken,
      TaskCreationOptions.LongRunning, TaskScheduler.Default);

    _ = Task.Factory.StartNew(() => _shutdownNodesQueue.ProcessBlocking(stoppingToken), stoppingToken,
      TaskCreationOptions.LongRunning, TaskScheduler.Default);
  }

  private async Task CleanupAndStopContainers(string[] ids, IInfrastructureClient client,
    INodeSnapshotRepository repo, IUnitOfWork uow, IImagesManager imagesManager, CancellationToken ct)
  {
    try
    {
      _logger.LogDebug("Stopping containers on nodes {@NodeIds}", (IEnumerable<string>)ids);
      var nodes = client.AliveNodes.ToArray();
      List<KeyValuePair<string, string>> userNodeIds = new(nodes.Length);
      foreach (var n in nodes.Where(_ => _.User is not null))
      {
        userNodeIds.Add(KeyValuePair.Create(n.User!.Id, n.Id));
        n.Unbind();
      }

      if (userNodeIds.Any())
      {
        await repo.RemoveUserNodes(userNodeIds, ct);
        await uow.SaveEntitiesAsync(ct);
      }

      await Parallel.ForEachAsync(nodes, ct,
        async (node, c) =>
        {
          await _dockerApiPolicy.ExecuteAsync(async () => await imagesManager.TryStopContainersAsync(node, c));
        });
      _logger.LogInformation("All containers were stopped {@NodeIds}", (IEnumerable<string>)ids);
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Error on stopping containers");
    }
    finally
    {
      foreach (var id in ids)
      {
        _scheduledContainerStops.TryRemove(id, out _);
      }
    }
  }

  private async Task StartContainers(string[] ids, CheckoutServiceSpawnConfig cfg, IImageInfoRepository imagesRepo,
    IInfrastructureClient client, IImagesManager imagesManager, CancellationToken ct)
  {
    try
    {
      _logger.LogDebug("Starting containers on nodes {@NodeIds}", (IEnumerable<string>)ids);
      var imgNames = new HashSet<string> { cfg.ImageName };
      var images = await imagesRepo.GetLatestByNames(imgNames, ct);
      var nodes = client.AliveNodes.Where(n => n.User is not null && ids.Contains(n.Id)).ToArray();
      var portBindings = new[] { cfg.PortBinding };

      await Parallel.ForEachAsync(nodes, ct, async (node, ctoken) =>
      {
        var img = images[cfg.ImageName];
        var env = cfg.RenderEnv(node.User!);
        await _dockerApiPolicy.ExecuteAsync(async () => await imagesManager.TryStopContainersAsync(node, ct));
        await _dockerApiPolicy.ExecuteAsync(async () =>
          await imagesManager.SpawnImageAsync(img, node, env, portBindings, ctoken));

        _logger.LogDebug("Started container on node {NodeId}", node.Id);
      });

      _logger.LogInformation("All containers were started {@NodeIds}", (IEnumerable<string>)ids);
    }
    catch (OperationCanceledException opCancExc)
    {
      _logger.LogWarning("Starting containers: " + opCancExc.Message);
    }
    catch (InvalidOperationException invOpExc)
    {
      _logger.LogWarning("Starting containers: " + invOpExc.Message);
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Error on starting containers");
    }
    finally
    {
      foreach (var id in ids)
      {
        _scheduledContainerStarts.TryRemove(id, out _);
      }
    }
  }

  private async Task ShutdownNodes(IInfrastructureClient client, string[] ids, CancellationToken ct)
  {
    try
    {
      _logger.LogDebug("Shutting down nodes {@NodeIds}", (IEnumerable<string>)ids);
      var nodesToTerminate = client.AvailableNodes.Where(n => ids.Contains(n.Id));
      await client.TerminateBatch(nodesToTerminate, ct);

      _logger.LogInformation("Nodes were shut down {@NodeIds}", (IEnumerable<string>)ids);
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Error on shutting-down nodes");
    }
    finally
    {
      foreach (var id in ids)
      {
        _shutdownNodesQueue.CompletedProcessing(id);
      }
    }
  }

  private async Task StartNewNodes(IReadOnlyCollection<string> ids, INodeSnapshotRepository repo,
    IInfrastructureClient client, IUnitOfWork uow, CancellationToken ct)
  {
    try
    {
      var snapshots = (await repo.GetPendingByUserIds(ids, ct)).ToDictionary(_ => _.User.Id);
      if (snapshots.Count == 0)
      {
        _logger.LogDebug("Nodes for users {@UserIds} already spawned", ids);
        return;
      }

      var alreadyBoundIds = client.AliveNodes.Where(_ => _.User is not null).Select(_ => _.Id).ToArray();
      await repo.RemoveByUserIds(alreadyBoundIds, ct);
      ids = ids.Except(alreadyBoundIds).ToArray();
      if (ids.Count == 0)
      {
        await uow.SaveEntitiesAsync(ct);
        _logger.LogDebug("Nodes for users {@UserIds} already bound", (IEnumerable<string>) alreadyBoundIds);
        return;
      }

      var currentNodeIds = client.AliveNodes.Select(_ => _.ToString());
      _logger.LogDebug("Starting new nodes for users {@UserIds}. (Available nodes: {@NodeIds})", ids, currentNodeIds);
      var nodes = await client.AcquireNewNodes(ids.Count, ct);
      var ix = 0;
      foreach (var id in ids)
      {
        var node = nodes[ix++];
        var snapshot = snapshots[id];
        node.Bind(snapshot, LifetimeStatus.ConnectionLost);
        _logger.LogInformation("Bound node {NodeId} to user {@User}", node.Id, snapshot.User);
      }

      var boundUserIDs = client.AvailableNodes
        .Where(n => n.User is not null)
        .Select(_ => _.User!.Id)
        .ToHashSet();
      var notBoundNodes = ids.Except(boundUserIDs);
      if (notBoundNodes.Any())
      {
        _logger.LogWarning("No bound node found for some users!!! {UserIds}", notBoundNodes);
      }

      repo.Remove(snapshots.Values);
      await uow.SaveEntitiesAsync(ct);
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Error on starting nodes");
    }
    finally
    {
      foreach (var id in ids)
      {
        _startNodesQueue.CompletedProcessing(id);
      }
    }
  }
}