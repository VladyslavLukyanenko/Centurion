namespace Centurion.CloudManager.Domain;

public interface INodeSnapshotRepository
{
  ValueTask<IList<NodeSnapshot>> GetListAsync(CancellationToken ct = default);
  ValueTask CreateAsync(IEnumerable<NodeSnapshot> nodes, CancellationToken ct = default);
  IList<NodeSnapshot> Update(IEnumerable<NodeSnapshot> nodes);
  void Remove(IEnumerable<NodeSnapshot> nodes);
  ValueTask<IList<NodeSnapshot>> GetByUserIds(IEnumerable<string> userIds, CancellationToken ct = default);
  ValueTask RemoveUserNodes(IEnumerable<KeyValuePair<string, string>> userNodeIds, CancellationToken ct = default);
  ValueTask RemoveByUserIds(IEnumerable<string> userIds, CancellationToken ct = default);
  ValueTask<IList<NodeSnapshot>> GetPendingByUserIds(IEnumerable<string> userIds, CancellationToken ct = default);
  ValueTask<IList<NodeSnapshot>> GetAllPending(CancellationToken ct = default);
}