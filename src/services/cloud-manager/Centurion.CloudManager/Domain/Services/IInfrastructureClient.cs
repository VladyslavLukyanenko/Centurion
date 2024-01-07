using DynamicData;

namespace Centurion.CloudManager.Domain.Services;

public interface IInfrastructureClient
{
  /*
   * GET running instances
   * GET instances statuses
   *
   * POST run stopped instance or new if there are no stopped at the moment
   * DELETE stop instance
   * DELETE /permanent terminate instance
   */

  IObservableCache<Node, string> Nodes { get; }
  IEnumerable<Node> AvailableNodes { get; }
  IEnumerable<Node> AliveNodes => AvailableNodes.Where(_ => _.IsAlive);

  string ProviderName { get; }
  bool HasAvailableNodes { get; }
  Task<IList<Node>> RefreshNodesAsync(CancellationToken ct = default);

  Task RunOrStartAsync(Node? node = null, CancellationToken ct = default);

  Task StopAsync(Node node, CancellationToken ct = default);

  Task TerminateAsync(Node node, CancellationToken ct = default);
  ValueTask TerminateBatch(IEnumerable<Node> nodes, CancellationToken ct = default);

  Node? GetRunningById(string nodeId);
  IList<Node> GetRangeRunningImage(ImageInfo image);
  Node? GetStoppedById(string nodeId);
  bool IsSupportedImageType(ImageInfo image);
  ValueTask<IList<Node>> AcquireNewNodes(int nodesCount, CancellationToken ct = default);
}