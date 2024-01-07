using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using Centurion.CloudManager.Web.Services;
using Centurion.Contracts.CloudManager;
using Centurion.SeedWork.Primitives;
using DynamicData;
using Google.Protobuf.WellKnownTypes;

namespace Centurion.CloudManager.App.Services;

public class AwsCloudManager : ICloudManager
{
  private static readonly SemaphoreSlim Locker = new(1, 1);

  private readonly IInfrastructureClient _infrastructure;
  private readonly INodeSnapshotRepository _nodeSnapshotRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly CheckoutServiceSpawnConfig _config;
  private readonly ILogger<AwsCloudManager> _logger;

  public AwsCloudManager(IEnumerable<IInfrastructureClient> infrastructureClients,
    INodeSnapshotRepository nodeSnapshotRepository, IUnitOfWork unitOfWork, CheckoutServiceSpawnConfig config,
    ILogger<AwsCloudManager> logger)
  {
    _nodeSnapshotRepository = nodeSnapshotRepository;
    _unitOfWork = unitOfWork;
    _config = config;
    _logger = logger;
    _infrastructure = infrastructureClients.First(_ => _.ProviderName == ProviderName);
  }

  public string ProviderName => Clouds.AWS;

  public IObservable<IChangeSet<NodeInfo, string>> NodesInfo => _infrastructure.Nodes
    .Connect(_ => _.IsAlive)
    .ObserveOn(ThreadPoolScheduler.Instance)
    .Transform(MapToNodeInfo);

  public async ValueTask KeepAlive(IEnumerable<KeyValuePair<string, KeepAliveCommand>> keepAliveCommands,
    CancellationToken ct = default)
  {
    var commands = new Dictionary<string, KeepAliveCommand>(keepAliveCommands);
    if (commands.Count == 0)
    {
      return;
    }

    // if (Locker.CurrentCount == 0)
    // {
    //   _logger.LogDebug("Throttling commands since processing in progress");
    //   return;
    // }

    try
    {
      await Locker.WaitAsync(CancellationToken.None);
      await FilterPendingSpawningNodes(commands, ct);

      if (commands.Count == 0)
      {
        _logger.LogDebug("All commands already processed. Spawn is pending.");
        return;
      }

      var reusableNodes = KeepAliveAndGetReusable(commands);
      if (!commands.Any())
      {
        return;
      }

      _logger.LogDebug("Processing rest commands {@UserIds}", commands.Keys);
      var reusedNodes = await ReuseOrScheduleNodes(commands, reusableNodes, ct);
      await _unitOfWork.SaveEntitiesAsync(ct);

      foreach (var node in reusedNodes)
      {
        node.ChangeStage(LifetimeStatus.Active);
      }
    }
    finally
    {
      Locker.Release();
    }
  }

  private LinkedList<Node> KeepAliveAndGetReusable(Dictionary<string, KeepAliveCommand> commands)
  {
    var reusableNodes = new LinkedList<Node>();
    foreach (var node in _infrastructure.AliveNodes)
    {
      if (node.User is not null && node.Stage.Status is LifetimeStatus.Active && commands.ContainsKey(node.User!.Id))
      {
        commands.Remove(node.User.Id);
        node.Stage.KeepAlive();
      }
      else if (node.IsEmptyPendingTermination)
      {
        reusableNodes.AddLast(node);
      }
      else if (node.IsConnectionLost(commands.Keys))
      {
        var cmd = commands[node.User!.Id];
        commands.Remove(cmd.UserId);
        node.ChangeStage(LifetimeStatus.Active);
        _logger.LogDebug("Connection restored for {@User} with node {NodeId}", node.User, node.Id);
      }

      if (commands.Count == 0)
      {
        break;
      }
    }

    if (reusableNodes.Count > 0)
    {
      _logger.LogDebug("Processed keepalive for active and found reusable nodes {NodeIds}",
        reusableNodes.Select(_ => _.Id));
    }

    return reusableNodes;
  }

  private async ValueTask<List<Node>> ReuseOrScheduleNodes(Dictionary<string, KeepAliveCommand> commands,
    LinkedList<Node> reusableNodes, CancellationToken ct = default)
  {
    var reusedNodes = new List<Node>(commands.Count);
    var snapshots = new List<NodeSnapshot>(commands.Count);
    var spawnUserIds = new List<string>(commands.Count);
    foreach (var user in commands.Values.Select(command => new UserInfo(command.UserId, command.UserName)))
    {
      if (reusableNodes.Count > 0)
      {
        var node = reusableNodes.Last!.Value;
        reusableNodes.RemoveLast();
        node.Reuse(user);
        reusedNodes.Add(node);
        _logger.LogDebug("Node {NodeId} reused by {@User}", node.Id, user);
      }
      else
      {
        snapshots.Add(NodeSnapshot.ScheduledAWS(user));
        spawnUserIds.Add(user.Id);
        _logger.LogDebug("Created pending snapshot for {@User}", user);
      }
    }

    if (spawnUserIds.Count > 0)
    {
      _logger.LogDebug("Removing pending spawns and creating snapshots {@UserIds}", spawnUserIds);
    }

    await _nodeSnapshotRepository.RemoveByUserIds(spawnUserIds, ct);
    await _nodeSnapshotRepository.CreateAsync(snapshots, ct);
    return reusedNodes;
  }

  private async Task FilterPendingSpawningNodes(Dictionary<string, KeepAliveCommand> commands, CancellationToken ct)
  {
    var pendingUserIds = (await _nodeSnapshotRepository.GetPendingByUserIds(commands.Keys, ct))
      .Select(_ => _.User.Id)
      .ToArray();

    foreach (var pendingUserId in pendingUserIds)
    {
      commands.Remove(pendingUserId);
    }

    if (pendingUserIds.Length > 0)
    {
      _logger.LogDebug("Filtered pending spawns for users {UserIds}", (IEnumerable<string>)pendingUserIds);
    }
  }

  private NodeInfo MapToNodeInfo(Node node)
  {
    var dnsName = "";
    if (node.DockerRemoteApiUrl is not null && !string.IsNullOrEmpty(node.PublicDnsName))
    {
      dnsName = _config.GetAbsoluteUrl(node).ToString();
    }

    return new NodeInfo
    {
      Status = node.ToNodeInfoStatus(),
      CreatedAt = node.CreatedAt.ToDateTimeOffset().ToTimestamp(),
      DnsName = dnsName,
      UserId = node.User?.Id ?? "",
      Images =
      {
        node.Images.Select(img =>
          {
            var data = new ImageInfoData
            {
              CreatedAt = img.CreatedAt.ToTimestamp(),
              ImageName = img.Name,
              ImageVersion = img.Version
            };

            foreach (var (key, value) in img.EnvironmentVariables)
            {
              data.Env.Add(key, value);
            }

            return data;
          })
          .ToArray()
      }
    };
  }
}