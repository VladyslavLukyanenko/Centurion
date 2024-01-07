using DynamicData;

namespace Centurion.CloudManager.Domain.Services;

public abstract class InfrastructureClientBase : IInfrastructureClient
{
  private readonly IImagesManager _imagesManager;
  private readonly IDockerClientsProvider _dockerClientsProvider;

  protected InfrastructureClientBase(string providerName, IImagesManager imagesManager,
    IDockerClientsProvider dockerClientsProvider)
  {
    _imagesManager = imagesManager;
    _dockerClientsProvider = dockerClientsProvider;
    ProviderName = providerName;
  }

  /*public async ValueTask UnbindBatch(IEnumerable<Node> nodes, CancellationToken ct = default)
  {
    foreach (var node in nodes)
    {
      node.Unbind();
      await ShutdownContainersAsync(node, ct);
    }
  }*/

  public async ValueTask TerminateBatch(IEnumerable<Node> nodes, CancellationToken ct = default)
  {
    var nodeList = nodes as Node[] ?? nodes.ToArray();
    await ShutDownNodes(nodeList, ct);
    foreach (var n in nodeList)
    {
      n.UpdateStatus(NodeStatus.ShuttingDown);
    }
  }

  public Node? GetRunningById(string nodeId) =>
    AvailableNodes.FirstOrDefault(_ => _.Status is NodeStatus.Running && _.Id == nodeId);

  public IList<Node> GetRangeRunningImage(ImageInfo image)
  {
    return AvailableNodes
      .Where(_ => _.Status is NodeStatus.Running && _.Images.Any(i => i.ImageId == image.Id))
      .ToList();
  }

  public Node? GetStoppedById(string nodeId)
  {
    return AvailableNodes.FirstOrDefault(_ => _.Status is NodeStatus.Stopped && _.Id == nodeId);
  }

  public async Task RunOrStartAsync(Node? node = null, CancellationToken ct = default)
  {
    await RunOrStartRemoteNode(node, ct);
    await RefreshNodesAsync(ct);
  }

  public async Task StopAsync(Node node, CancellationToken ct = default)
  {
    await ShutdownContainersAsync(node, ct);
    await StopRemoteNode(node, ct);
    await RefreshNodesAsync(ct);
  }

  public async Task TerminateAsync(Node node, CancellationToken ct = default)
  {
    await TerminateRemoteNode(node, ct);

    await RefreshNodesAsync(ct);
  }

  public abstract Task<IList<Node>> RefreshNodesAsync(CancellationToken ct = default);
  public abstract bool IsSupportedImageType(ImageInfo image);

  public abstract ValueTask<IList<Node>> AcquireNewNodes(int nodesCount, CancellationToken ct = default);

  protected abstract Task RunOrStartRemoteNode(Node? node = null, CancellationToken ct = default);
  protected abstract Task StopRemoteNode(Node node, CancellationToken ct = default);
  protected abstract Task ShutDownNodes(IEnumerable<Node> nodes, CancellationToken ct = default);
  protected abstract Task TerminateRemoteNode(Node node, CancellationToken ct = default);

  private async Task ShutdownContainersAsync(Node node, CancellationToken ct)
  {
    foreach (var info in node.Images.ToArray())
    {
      await _imagesManager.TryStopContainerAsync(node, info, ct);
    }

    node.ClearImages();
    node.PublicDnsName = null;
    node.Checked(false);
    _dockerClientsProvider.RemoveClient(node);
  }

  public IEnumerable<Node> AvailableNodes => Nodes.Items;
  public string ProviderName { get; }

  public abstract IObservableCache<Node, string> Nodes { get; }
  public bool HasAvailableNodes => AvailableNodes.Any(_ => _.IsReady);
}