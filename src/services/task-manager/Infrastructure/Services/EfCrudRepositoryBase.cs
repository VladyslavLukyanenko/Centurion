using Centurion.SeedWork.Primitives;
using Centurion.TaskManager.Core;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Services;

public abstract class EfCrudRepositoryBase<TEntity> : EfCrudRepositoryBase<TEntity, Guid>
  where TEntity : class, IEntity<Guid>, IUserProperty
{
  protected EfCrudRepositoryBase(DbContext ctx) : base(ctx)
  {
  }
}

public abstract class EfCrudRepositoryBase<TEntity, TKey>
  where TEntity : class, IEntity<TKey>, IUserProperty
{
  protected readonly DbContext Ctx;

  protected EfCrudRepositoryBase(DbContext ctx)
  {
    Ctx = ctx;
  }

  public async ValueTask<TEntity?> GetByIdAsync(TKey id, string userId, CancellationToken ct = default)
  {
    return await Ctx.Set<TEntity>().FirstOrDefaultAsync(_ => Equals(_.Id, id) && _.UserId == userId, ct);
  }

  public async ValueTask<TEntity> CreateAsync(TEntity entity, CancellationToken ct = default)
  {
    var e = await Ctx.AddAsync(entity, ct);
    return e.Entity;
  }

  public async ValueTask CreateAsync(IEnumerable<TEntity> aggregate, CancellationToken cancellationToken = default)
  {
    await Ctx.Set<TEntity>().AddRangeAsync(aggregate, cancellationToken);
  }

  public void Update(TEntity entity)
  {
    Ctx.Update(entity);
  }

  public void Update(IEnumerable<TEntity> entities)
  {
    Ctx.UpdateRange(entities);
  }

  public void Remove(TEntity entity)
  {
    Ctx.Remove(entity);
  }

  public void Remove(IEnumerable<TEntity> entities)
  {
    Ctx.RemoveRange(entities);
  }
}