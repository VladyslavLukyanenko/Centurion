using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Infra.Repositories;

public abstract class EfReadRepository<T, TKey> : IRepository<T, TKey>
  where T : class, IEntity<TKey> where TKey : IComparable<TKey>, IEquatable<TKey>
{
  protected readonly DbContext Context;

  protected EfReadRepository(DbContext context, IUnitOfWork unitOfWork)
  {
    UnitOfWork = unitOfWork;
    Context = context;
  }

  public IUnitOfWork UnitOfWork { get; }

  protected virtual IQueryable<T> DataSource => Context.Set<T>();

  // private IQueryable<T> CacheableDataSource => DataSource
  // .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromSeconds(15));

  public async ValueTask<T?> GetByIdAsync(TKey id, CancellationToken ct = default)
  {
    var local = Context.ChangeTracker.Entries<T>()
      .Where(_ => Equals(_.Entity.Id, id))
      .Select(_ => _.Entity)
      .FirstOrDefault();

    return local ?? await DataSource.FirstOrDefaultAsync(_ => _.Id.Equals(id), ct)!;
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
}