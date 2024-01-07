using System.Linq.Expressions;
using Centurion.SeedWork.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Services;

public abstract class EfReadRepository<T, TKey>
  : IRepository<T, TKey>
  where T : class, IEntity<TKey> 
  where TKey : IComparable<TKey>, IEquatable<TKey>
{
  protected readonly DbContext Context;

  protected EfReadRepository(DbContext context, IUnitOfWork unitOfWork)
  {
    UnitOfWork = unitOfWork;
    Context = context;
  }

  public IUnitOfWork UnitOfWork { get; }

  protected virtual IQueryable<T> DataSource => Context.Set<T>();

  public async ValueTask<T?> GetByIdAsync(TKey id, CancellationToken ct = default)
  {
    return await Context.FindAsync<T>(id);
    // return DataSource.FirstOrDefaultAsync(_ => _.Id.Equals(id), ct)!;
  }

  public async ValueTask<IList<T>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default)
  {
    return await DataSource.Where(_ => ids.Contains(_.Id))
      .ToListAsync(ct);
  }

  public async ValueTask<bool> ExistsAsync(TKey key, CancellationToken token = default)
  {
    return await DataSource.AnyAsync(_ => _.Id.Equals(key), token);
  }

  public async ValueTask<IList<T>> ListAllAsync(CancellationToken token = default)
  {
    return await DataSource.ToListAsync(token);
  }

  public async Task<T?> FindAsync(CancellationToken token = default, params object[] id)
  {
    return await Context.Set<T>().FindAsync(id);
  }

  public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default)
  {
    return DataSource.AnyAsync(predicate, token);
  }
}