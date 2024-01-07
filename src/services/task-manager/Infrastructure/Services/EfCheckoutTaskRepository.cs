using Centurion.TaskManager.Core;
using Centurion.TaskManager.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfCheckoutTaskRepository : EfCrudRepositoryBase<CheckoutTask>, ICheckoutTaskRepository
{
  public EfCheckoutTaskRepository(DbContext ctx) : base(ctx)
  {
  }

  public async ValueTask<IReadOnlyList<CheckoutTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, string userId,
    CancellationToken ct = default)
  {
    return await Ctx.Set<CheckoutTask>()
      .Where(_ => _.UserId == userId && taskIds.Contains(_.Id))
      .OrderByDescending(_ => _.UpdatedAt)
      .ToArrayAsync(ct);
  }

  public async ValueTask<IReadOnlyList<CheckoutTask>> GetByIdsAsync(IEnumerable<Guid> taskIds, Guid groupId, CancellationToken ct = default)
  {
    return await Ctx.Set<CheckoutTask>()
      .Where(_ => _.GroupId == groupId && taskIds.Contains(_.Id))
      .OrderByDescending(_ => _.UpdatedAt)
      .ToArrayAsync(ct);
  }

  public async ValueTask<IList<CheckoutTask>> GetByGroupIdAsync(Guid groupId, string userId,
    CancellationToken ct = default)
  {
    return await Ctx.Set<CheckoutTask>()
      .Where(_ => _.UserId == userId && _.GroupId == groupId)
      .ToListAsync(ct);
  }
}