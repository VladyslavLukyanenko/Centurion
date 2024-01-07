using Centurion.SeedWork.Primitives;
using CSharpFunctionalExtensions;
using DynamicData;

namespace Centurion.Cli.Core.Services;

public interface IRepository<T, TKey> : IAppStateHolder
  where T : IEntity<TKey>
  where TKey : IEquatable<TKey>
{
  IObservableCache<T, TKey> Items { get; }
  ValueTask<Result<T>> SaveAsync(T item, CancellationToken ct = default);
  ValueTask<Result<T>> SaveSilentlyAsync(T item, CancellationToken ct = default);
  ValueTask<Result<IList<T>>> SaveAsync(IEnumerable<T> items, CancellationToken ct = default);
  ValueTask<Result> RemoveAsync(T toRemove, CancellationToken ct = default);
  IEnumerable<T> LocalItems { get; }
  ValueTask<Result<IList<T>>> GetAllAsync(CancellationToken ct = default);
}