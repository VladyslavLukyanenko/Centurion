using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using DynamicData;
using Google.Apis.Compute.v1;
using Google.Apis.Compute.v1.Data;

namespace Centurion.CloudManager.Infra.Services.Gcp;

public class GcpInfrastructureClient : InfrastructureClientBase
{
  private static readonly SourceCache<Node, string> NodesCache = new(_ => _.Id);

  private readonly GcpConfig _config;
  private readonly ComputeService _computeService;

  public GcpInfrastructureClient(GcpConfig config, ComputeService computeService,
    IImagesManager imagesManager, IDockerClientsProvider dockerClientsProvider)
    : base(Clouds.GCP, imagesManager, dockerClientsProvider)
  {
    _config = config;
    _computeService = computeService;
  }

  public override IObservableCache<Node, string> Nodes => NodesCache;

  public override async Task<IList<Node>> RefreshNodesAsync(CancellationToken ct = default)
  {
    const int maxResults = 1_000;
    string? nextToken = null;
    List<Instance> srcInstances = new(maxResults);
    do
    {
      var request = _computeService.Instances.List(_config.ProjectId, _config.Zone);
      request.Filter = "name:gcp.checkoutapi.*";
      if (!string.IsNullOrEmpty(nextToken))
      {
        request.PageToken = nextToken;
      }

      var instancesResponse = await request.ExecuteAsync(ct);

      srcInstances.EnsureCapacity(srcInstances.Count + instancesResponse.Items.Count);
      srcInstances.AddRange(instancesResponse.Items);
      nextToken = instancesResponse.NextPageToken;
    } while (!string.IsNullOrEmpty(nextToken));

    return MapInstancesAndMerge(srcInstances);
  }

  private List<Node> MapInstancesAndMerge(IList<Instance> instances)
  {
    var mappedInstances = instances
      // .Where(_ => _.KeyName == _awsConfig.KeyName) // filter by source template
      .Select(MapToGcpNodes)
      .ToList();

    NodesCache.Edit(cache =>
    {
      cache.Load(mappedInstances);
    });

    return mappedInstances;
  }

  public override bool IsSupportedImageType(ImageInfo image) => true;

  public override async ValueTask<IList<Node>> AcquireNewNodes(int nodesCount, CancellationToken ct = default)
  {
    if (nodesCount <= 0)
    {
      throw new InvalidOperationException("Can't start zero or less nodes");
    }

    var groupId = Guid.NewGuid().ToString("N");
    var insertRequest = _computeService.Instances.BulkInsert(new BulkInsertInstanceResource
    {
      Count = nodesCount,
      MinCount = nodesCount,
      NamePattern = "gcp.checkoutapi.######",
      InstanceProperties = new InstanceProperties
      {
        MachineType = $"zones/{_config.Zone}/machineTypes/{_config.MachineType}",
        Tags = new Tags
        {
          Items = new List<string>
          {
            "CheckoutAPINode",
            groupId
          }
        }
      },
      SourceInstanceTemplate = "global/instanceTemplates/" + _config.SourceInstanceTemplate
    }, _config.ProjectId, _config.Zone);

    var r = await insertRequest.ExecuteAsync(ct);
    if (r.Error is not null)
    {
      throw new InvalidOperationException(string.Join("\n", r.Error.Errors.Select(_ => _.Message)));
    }

    var nodes = await RefreshNodesAsync(ct);
    return nodes
      .Where(it => ((GcpNode)it).Tags.Contains(groupId))
      .ToList();
  }

  protected override async Task RunOrStartRemoteNode(Node? instance = null,
    CancellationToken ct = default)
  {
    if (instance != null)
    {
      if (instance.Status is not NodeStatus.Stopped)
      {
        throw new InvalidOperationException(
          $"Server should be stopped to start it. Id: {instance.Id}, Status: {instance.Status}");
      }

      var startRequest = _computeService.Instances.Start(_config.ProjectId, _config.Zone, instance.Id);
      await startRequest.ExecuteAsync(ct);
    }
    else
    {
      var instanceToCreate = new Instance
      {
        MachineType = $"zones/{_config.Zone}/machineTypes/{_config.MachineType}",
        Zone = _config.Zone,
        Name = "gcp.checkoutapi." + Guid.NewGuid().ToString("N"),
        Tags = new Tags
        {
          Items = new List<string>
          {
            "http-server"
          }
        }
      };
      var insertRequest = _computeService.Instances.Insert(instanceToCreate, _config.ProjectId, _config.Zone);
      insertRequest.SourceInstanceTemplate = "global/instanceTemplates/" + _config.SourceInstanceTemplate;
      await insertRequest.ExecuteAsync(ct);
    }
  }

  protected override Task StopRemoteNode(Node instance, CancellationToken ct = default)
  {
    var stopRequest = _computeService.Instances.Stop(_config.ProjectId, _config.Zone, instance.Id);
    return stopRequest.ExecuteAsync(ct);
  }

  protected override async Task ShutDownNodes(IEnumerable<Node> nodes, CancellationToken ct = default)
  {
    foreach (var node in nodes)
    {
      await TerminateRemoteNode(node, ct);
    }
  }

  protected override async Task TerminateRemoteNode(Node instance, CancellationToken ct = default)
  {
    // await StopRemoteNode(instance, ct);
    var deleteRequest = _computeService.Instances.Delete(_config.ProjectId, _config.Zone, instance.Id);
    await deleteRequest.ExecuteAsync(ct);
  }

  private Node MapToGcpNodes(Instance i)
  {
    var existing = NodesCache.Lookup(i.Name);

    if (!existing.HasValue)
    {
      return new GcpNode(i);
    }

    var gcpNode = (GcpNode)existing.Value;
    gcpNode.UpdateFromSource(i);
    return gcpNode;
  }
}