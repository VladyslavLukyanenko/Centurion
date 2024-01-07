namespace Centurion.Accounts.Core.Primitives;

public interface ICrudRepository<T, in TKey>
  : IRepository<T, TKey>
  where T : class, IEntity<TKey>
  where TKey : IComparable<TKey>, IEquatable<TKey>
{
  ValueTask<T> CreateAsync(T aggregate, CancellationToken ct = default);
  ValueTask CreateAsync(IEnumerable<T> aggregate, CancellationToken ct = default);

  T Update(T aggregate);
  IList<T> Update(IEnumerable<T> entities);

  void Remove(T aggregate);
  void Remove(IEnumerable<T> aggregates);


  object SaveGraph(object rootGraphEntity);
  object SaveGraph(object rootGraphEntity, object existentEntity);
}