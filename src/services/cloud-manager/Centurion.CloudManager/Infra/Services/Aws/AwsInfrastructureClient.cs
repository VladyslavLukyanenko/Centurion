using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using DynamicData;

namespace Centurion.CloudManager.Infra.Services.Aws;

// todo: implement fault tolerance + retry policy
public class AwsInfrastructureClient : InfrastructureClientBase
{
  private static readonly SourceCache<Node, string> NodesCache = new(_ => _.Id);
  private static readonly SemaphoreSlim RefreshGates = new(1, 1);
  private static readonly SemaphoreSlim AcquireGates = new(1, 1);

  private readonly AwsConfig _awsConfig;
  private readonly AWSCredentials _awsCredentials;
  private readonly RegionEndpoint _awsRegionEndpoint;
  private readonly ILogger<AwsInfrastructureClient> _logger;

  public AwsInfrastructureClient(AwsConfig awsConfig, IImagesManager imagesManager,
    IDockerClientsProvider dockerClientsProvider, ILogger<AwsInfrastructureClient> logger)
    : base(Clouds.AWS, imagesManager, dockerClientsProvider)
  {
    _awsConfig = awsConfig;
    _logger = logger;
    _awsCredentials = new BasicAWSCredentials(awsConfig.AccessKeyId, awsConfig.SecretAccessKey);
    _awsRegionEndpoint = RegionEndpoint.GetBySystemName(_awsConfig.PlacementRegion);
  }

  public override IObservableCache<Node, string> Nodes => NodesCache;

  public override async Task<IList<Node>> RefreshNodesAsync(CancellationToken ct = default)
  {
    try
    {
      await RefreshGates.WaitAsync(CancellationToken.None);
      using var amazonEc2Client = CreateAwsEc2Client();
      const int maxResults = 1_000;
      string? nextToken = null;
      List<Instance> srcInstances = new(maxResults);
      do
      {
        var instancesResponse = await amazonEc2Client.DescribeInstancesAsync(new DescribeInstancesRequest
        {
          NextToken = nextToken,
          MaxResults = maxResults,
          Filters = new List<Filter>
          {
            new()
            {
              Name = "key-name",
              Values = new List<string>
              {
                _awsConfig.KeyName
              }
            }
          }
        }, ct);

        srcInstances.EnsureCapacity(srcInstances.Count + instancesResponse.Reservations.Count);
        srcInstances.AddRange(instancesResponse.Reservations.SelectMany(_ => _.Instances));
        nextToken = instancesResponse.NextToken;
      } while (!string.IsNullOrEmpty(nextToken));

      return MapInstancesAndMerge(srcInstances);
    }
    finally
    {
      RefreshGates.Release();
    }
  }

  private List<Node> MapInstancesAndMerge(IEnumerable<Instance> srcInstances)
  {
    var mappedInstances = srcInstances
      .Where(_ => _.KeyName == _awsConfig.KeyName)
      .Select(MapToAwsServerInstance)
      .ToList();

    NodesCache.Edit(cache => { cache.Load(mappedInstances); });

    return mappedInstances;
  }

  public override bool IsSupportedImageType(ImageInfo image) => true;

  public override async ValueTask<IList<Node>> AcquireNewNodes(int nodesCount, CancellationToken ct = default)
  {
    try
    {
      await AcquireGates.WaitAsync(CancellationToken.None);
      if (nodesCount <= 0)
      {
        throw new InvalidOperationException("Can't start zero or less nodes");
      }

      using var amazonEc2Client = CreateAwsEc2Client();
      var started = await amazonEc2Client.RunInstancesAsync(
        new RunInstancesRequest(_awsConfig.AmiId, nodesCount, nodesCount)
        {
          InstanceType = InstanceType.FindValue(_awsConfig.InstanceType),
          KeyName = _awsConfig.KeyName,
          SecurityGroupIds = _awsConfig.SecurityGroupIds.ToList()
        }, ct);

      await RefreshNodesAsync(ct);
      return started.Reservation.Instances
        .Where(_ => _.KeyName == _awsConfig.KeyName)
        .Select(MapToAwsServerInstance)
        .ToList();
    }
    finally
    {
      AcquireGates.Release();
    }
  }

  protected override async Task RunOrStartRemoteNode(Node? node = null, CancellationToken ct = default)
  {
    using var amazonEc2Client = CreateAwsEc2Client();
    if (node != null)
    {
      if (node.Status is not NodeStatus.Stopped)
      {
        throw new InvalidOperationException(
          $"Server should be stopped to start it. Id: {node.Id}, Status: {node.Status}");
      }

      await amazonEc2Client.StartInstancesAsync(new StartInstancesRequest(new List<string> { node.Id }), ct);
      return;
    }

    await amazonEc2Client.RunInstancesAsync(new RunInstancesRequest(_awsConfig.AmiId, 1, 1)
    {
      InstanceType = InstanceType.FindValue(_awsConfig.InstanceType),
      KeyName = _awsConfig.KeyName,
      SecurityGroupIds = _awsConfig.SecurityGroupIds.ToList()
    }, ct);
  }

  protected override async Task StopRemoteNode(Node node, CancellationToken ct = default)
  {
    using var amazonEc2Client = CreateAwsEc2Client();
    await amazonEc2Client.StopInstancesAsync(new StopInstancesRequest(new List<string> { node.Id }), ct);
  }

  protected override async Task ShutDownNodes(IEnumerable<Node> nodes, CancellationToken ct = default)
  {
    var request = new TerminateInstancesRequest(nodes.Select(_ => _.Id).ToList());
    using var amazonEc2Client = CreateAwsEc2Client();
    await amazonEc2Client.TerminateInstancesAsync(request, ct);
  }

  protected override async Task TerminateRemoteNode(Node node, CancellationToken ct = default)
  {
    var request = new TerminateInstancesRequest(new List<string> { node.Id });
    using var amazonEc2Client = CreateAwsEc2Client();
    await amazonEc2Client.TerminateInstancesAsync(request, ct);
  }

  private Node MapToAwsServerInstance(Instance i)
  {
    var existing = NodesCache.Lookup(i.InstanceId);

    if (!existing.HasValue)
    {
      _logger.LogDebug("New node spawned {NodeId}", i.InstanceId);
      return new AwsNode(i);
    }

    var awsNode = (AwsNode)existing.Value;
    awsNode.UpdateFromSource(i);
    if (awsNode.User is not null)
    {
      _logger.LogDebug("Updating Node {Node}", awsNode.ToString());
    }

    return awsNode;
  }

  private AmazonEC2Client CreateAwsEc2Client() => new(_awsCredentials, _awsRegionEndpoint);
}