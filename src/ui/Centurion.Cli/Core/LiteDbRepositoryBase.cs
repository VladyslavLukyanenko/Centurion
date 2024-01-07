using System.Reactive.Linq;
using Centurion.SeedWork.Primitives;
using CSharpFunctionalExtensions;
using DynamicData;
using LiteDB;

namespace Centurion.Cli.Core;

public abstract class LiteDbRepositoryBase<T> : LiteDbRepositoryBase<T, Guid>
  where T : class, IEntity<Guid>
{
  protected LiteDbRepositoryBase(ILiteDatabase database) : base(database)
  {
  }
}

public abstract class LiteDbRepositoryBase<T, TKey> : Services.IRepository<T, TKey>
  where T : class, IEntity<TKey> where TKey : IComparable<TKey>, IEquatable<TKey>
{
  protected readonly ILiteDatabase Database;
  protected readonly ISourceCache<T, TKey> Cache = new SourceCache<T, TKey>(_ => _.Id);
  protected readonly ILiteCollection<T> Collection;

  protected LiteDbRepositoryBase(ILiteDatabase database)
  {
    Database = database;
    Collection = database.GetCollection<T>();
    Items = Cache.AsObservableCache();
  }

  public IObservable<bool> IsFetching { get; } = Observable.Return(false);

  public ValueTask<Result> InitializeAsync(CancellationToken ct = default)
  {
    var items = Collection.FindAll();
    Cache.AddOrUpdate(items);

    return ValueTask.FromResult(Result.Success());
  }

  public void ResetCache()
  {
    Cache.Clear();
  }

  public IObservableCache<T, TKey> Items { get; }

  public virtual ValueTask<Result<T>> SaveAsync(T item, CancellationToken ct = default)
  {
    Collection.Upsert(item);
    Cache.AddOrUpdate(item);
    Database.Checkpoint();
    return ValueTask.FromResult<Result<T>>(item);
  }

  public ValueTask<Result<T>> SaveSilentlyAsync(T item, CancellationToken ct = default)
  {
    Collection.Upsert(item);
    return ValueTask.FromResult<Result<T>>(item);
  }

  public ValueTask<Result<IList<T>>> SaveAsync(IEnumerable<T> items, CancellationToken ct = default)
  {
    var toSave = items.ToList();
    Collection.Upsert(toSave);
    Cache.AddOrUpdate(toSave);
    Database.Checkpoint();

    return ValueTask.FromResult<Result<IList<T>>>(toSave);
  }

  public ValueTask<Result> RemoveAsync(T toRemove, CancellationToken ct = default)
  {
    Cache.Remove(toRemove);
    Collection.Delete(new BsonValue(toRemove.Id));
    Database.Checkpoint();
    return ValueTask.FromResult(Result.Success());
  }

  public IEnumerable<T> LocalItems => Cache.Items;

  public async ValueTask<Result<IList<T>>> GetAllAsync(CancellationToken ct = default)
  {
    if (Cache.Count == 0)
    {
      await InitializeAsync(ct);
    }

    return LocalItems.ToList();
  }
}