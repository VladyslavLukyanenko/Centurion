using Centurion.CloudManager.Domain;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Services;
using Centurion.SeedWork.Primitives;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Centurion.CloudManager.Infra.Repositories;

public class EfNodeSnapshotRepository : EfCrudRepository<NodeSnapshot>, INodeSnapshotRepository
{
  public EfNodeSnapshotRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public async ValueTask<IList<NodeSnapshot>> GetListAsync(CancellationToken ct = default)
  {
    return await DataSource.ToListAsync(ct);
  }

  public async ValueTask<IList<NodeSnapshot>> GetByUserIds(IEnumerable<string> userIds, CancellationToken ct = default)
  {
    return await DataSource.Where(_ => userIds.Contains(_.User.Id)).ToListAsync(ct);
  }

  public async ValueTask RemoveUserNodes(IEnumerable<KeyValuePair<string, string>> userNodeIds,
    CancellationToken ct = default)
  {
    const string delim = "__";
    var joined = userNodeIds.Select(k => k.Key + delim + k.Value);
    await DataSource.Where(_ => joined.Contains(_.User.Id + delim + _.NodeId))
      .DeleteAsync(ct);
  }

  public async ValueTask RemoveByUserIds(IEnumerable<string> userIds, CancellationToken ct = default) =>
    await DataSource.Where(_ => userIds.Contains(_.User.Id))
      .DeleteAsync(ct);

  public async ValueTask<IList<NodeSnapshot>> GetPendingByUserIds(IEnumerable<string> userIds,
    CancellationToken ct = default) =>
    await DataSource.Where(_ => string.IsNullOrEmpty(_.NodeId) && userIds.Contains(_.User.Id))
      .ToListAsync(ct);

  public async ValueTask<IList<NodeSnapshot>> GetAllPending(CancellationToken ct = default) =>
    await DataSource.Where(_ => string.IsNullOrEmpty(_.NodeId)).ToListAsync(ct);
}