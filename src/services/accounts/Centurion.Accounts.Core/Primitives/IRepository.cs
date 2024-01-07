namespace Centurion.Accounts.Core.Primitives;

public interface IRepository<T, in TKey>
  where T : class, IEntity<TKey>
  where TKey : IComparable<TKey>, IEquatable<TKey>
{
  ValueTask<T?> GetByIdAsync(TKey id, CancellationToken ct = default);
  ValueTask<IList<T>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default);
  ValueTask<bool> ExistsAsync(TKey key, CancellationToken token = default);
  ValueTask<IList<T>> ListAllAsync(CancellationToken token = default);
}