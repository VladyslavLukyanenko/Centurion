using Centurion.Cli.Core.Services.Modules;
using Centurion.SeedWork.Primitives;
using CSharpFunctionalExtensions;
using DynamicData;

namespace Centurion.Cli.Core.Services;

public abstract class RemoteRepositoryBase<T, TKey> : ExecutionStatusProviderBase, IRepository<T, TKey>
  where T : IEntity<TKey>
  where TKey : IEquatable<TKey>
{
  protected readonly ISourceCache<T, TKey> Cache = new SourceCache<T, TKey>(_ => _.Id);

  protected RemoteRepositoryBase()
  {
    Items = Cache.AsObservableCache();
  }

  public ValueTask<Result> InitializeAsync(CancellationToken ct = default) => Guard.ExecuteSafe(async () =>
  {
    var items = await FetchAllAsync(ct).TrackProgress(FetchingTracker);
    Cache.AddOrUpdate(items);
  });

  public void ResetCache()
  {
    Cache.Clear();
  }

  public IObservableCache<T, TKey> Items { get; }

  public virtual ValueTask<Result<T>> SaveAsync(T item, CancellationToken ct = default) => Guard.ExecuteSafeFunc(
      async () =>
      {
        item = await SaveRemoteAsync(item, ct);
        Cache.AddOrUpdate(item);
        return item;
      })
    .TrackProgress(FetchingTracker);

  public ValueTask<Result<T>> SaveSilentlyAsync(T item, CancellationToken ct = default) =>
    Guard.ExecuteSafeFunc(async () => await SaveRemoteAsync(item, ct))
      .TrackProgress(FetchingTracker);

  public ValueTask<Result<IList<T>>> SaveAsync(IEnumerable<T> items, CancellationToken ct = default)
  {
    // var toSave = items.ToList();
    // Collection.Upsert(toSave);
    // Cache.AddOrUpdate(toSave);
    //
    // return default;
    throw new NotImplementedException();
  }

  public ValueTask<Result> RemoveAsync(T toRemove, CancellationToken ct = default) => Guard.ExecuteSafe(async () =>
  {
    Cache.Remove(toRemove);
    await RemoveRemoteAsync(toRemove, ct).TrackProgress(FetchingTracker);
  });

  public IEnumerable<T> LocalItems => Cache.Items;

  public async ValueTask<Result<IList<T>>> GetAllAsync(CancellationToken ct = default)
  {
    if (Cache.Count == 0)
    {
      var result = await InitializeAsync(ct).TrackProgress(FetchingTracker);
      if (result.IsFailure)
      {
        return result.ConvertFailure<IList<T>>();
      }
    }

    return LocalItems.ToList();
  }

  protected abstract ValueTask<IList<T>> FetchAllAsync(CancellationToken ct);
  protected abstract ValueTask<T> SaveRemoteAsync(T item, CancellationToken ct);
  protected abstract ValueTask RemoveRemoteAsync(T item, CancellationToken ct);
}