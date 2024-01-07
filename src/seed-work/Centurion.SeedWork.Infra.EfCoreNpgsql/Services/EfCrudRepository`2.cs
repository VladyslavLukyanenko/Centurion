using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Centurion.SeedWork.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Services;

public abstract class EfCrudRepository<T, TKey>
  : EfReadRepository<T, TKey>, ICrudRepository<T, TKey>
  where T : class, IEntity<TKey>
  where TKey : IComparable<TKey>, IEquatable<TKey>
{
  private static readonly IDictionary<Type, MethodInfo> AddMethodsCache =
    new ConcurrentDictionary<Type, MethodInfo>();

  private static readonly IDictionary<Type, MethodInfo> ContainsMethodsCache =
    new ConcurrentDictionary<Type, MethodInfo>();

  protected EfCrudRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public async ValueTask<T> CreateAsync(T aggregate, CancellationToken cancellationToken = default)
  {
    var entry = await Context.Set<T>().AddAsync(aggregate, cancellationToken);
    return entry.Entity;
  }

  public async ValueTask CreateAsync(IEnumerable<T> aggregate, CancellationToken cancellationToken = default)
  {
    await Context.Set<T>().AddRangeAsync(aggregate, cancellationToken);
  }

  public T Update(T aggregate)
  {
    var e = Context.Update(aggregate).Entity;
//            var e = Context.UpdateEntity<T, TKey>(aggregate).Entity;
    return e;
  }

  public object SaveGraph(object rootGraphEntity)
  {
    Type t = rootGraphEntity.GetType();

    EntityEntry? entry = Context.ChangeTracker.Entries()
      .Where(e => e.Entity.GetType() == t)
      .FirstOrDefault(attachedEntry => AreIdsEqual(rootGraphEntity, attachedEntry));

    return SaveGraph(rootGraphEntity, entry).Entity;
  }

  public object SaveGraph(object rootGraphEntity, object? existentEntity)
  {
    if (existentEntity == null)
    {
      return SaveGraph(rootGraphEntity);
    }

    var existentEntityEntry = Context.Entry(existentEntity);
    var entry = SaveGraph(rootGraphEntity, existentEntityEntry);

    return entry.Entity;
  }

  public virtual IList<T> Update(IEnumerable<T> entities)
  {
    return entities.Select(Update).ToList();
  }

  public virtual void Remove(T aggregate)
  {
    MarkRemoved(aggregate);
  }

  public virtual void Remove(IEnumerable<T> aggregates)
  {
    var entities = aggregates as T[] ?? aggregates.ToArray();
    MarkRemoved(entities);
  }

  private static bool AreIdsEqual(object saveEntity, EntityEntry attachedEntry)
  {
    return GetPrimaryKeys(attachedEntry)
      .All(prop => Equals(prop.GetValue(attachedEntry.Entity), prop.GetValue(saveEntity)));
  }

  private static bool AreIdsEqual(object saveEntity, object existent, IEnumerable<PropertyInfo> ids)
  {
    return ids.All(prop => Equals(prop.GetValue(existent), prop.GetValue(saveEntity)));
  }

  private static IEnumerable<PropertyInfo> GetPrimaryKeys(EntityEntry attachedEntry)
  {
    return attachedEntry.Metadata.GetKeys().Where(key => key.IsPrimaryKey())
      .SelectMany(key =>
        key.Properties
          .Where(p => !p.IsShadowProperty())
          .Select(p => p.PropertyInfo!)
      );
  }

  private EntityEntry SaveGraph(object rootGraphEntity, EntityEntry? entry)
  {
    if (entry != null)
    {
      if (HasChanges(entry, entry.Entity, rootGraphEntity))
      {
        entry.CurrentValues.SetValues(rootGraphEntity);
      }
    }
    else
    {
      entry = Context.Add(rootGraphEntity);
    }

    foreach (MemberEntry member in entry.Members)
    {
      if (member is PropertyEntry property)
      {
        if (property.Metadata.IsConcurrencyToken)
        {
          var oldPropertyName = property.Metadata.Name;
          var oldProperty = entry.Property(oldPropertyName);
          property.CurrentValue = oldProperty.CurrentValue;
          property.OriginalValue = oldProperty.OriginalValue;
        }
      }
      else if (member is ReferenceEntry navigation)
      {
        var getter = navigation.Metadata.PropertyInfo;

        var currentNavigationValue = getter!.GetValue(rootGraphEntity)!;
        SaveGraph(currentNavigationValue, navigation.TargetEntry);
      }
      else if (member is CollectionEntry collection)
      {
        var collectionProp = collection.Metadata.PropertyInfo!;
        var currentCollection = ((IEnumerable) collectionProp.GetValue(rootGraphEntity)!)
          .OfType<object>()
          .ToList();

        foreach (var item in currentCollection)
        {
          var saved = SaveGraph(item);
          var savedChildEntry = Context.Entry(saved);
          if (savedChildEntry.State == EntityState.Added && !ExistsInCollection(collection, saved))
          {
            AddToCollection(collection, saved);
          }
        }

        var list = collection.CurrentValue!.OfType<object>().ToList();
        if (list.Count == 0)
        {
          return entry;
        }
          
        var childEntry = Context.Entry(list[0]);
        List<PropertyInfo> pks = GetPrimaryKeys(childEntry).ToList();
        foreach (var existent in list
                   .Where(existent => !currentCollection.Any(current => AreIdsEqual(current, existent, pks))))
        {
          Context.Remove(existent);
        }
      }
    }

    return entry;
  }

  private bool ExistsInCollection(CollectionEntry collection, object saved)
  {
    var type = collection.CurrentValue!.GetType();
    if (!ContainsMethodsCache.TryGetValue(type, out var existsInCollectionMethod))
    {
      existsInCollectionMethod =
        type.GetMethod(nameof(ICollection<T>.Contains))
        ?? throw new InvalidOperationException("Can't find method " + nameof(ICollection<T>.Contains));
      ContainsMethodsCache[type] = existsInCollectionMethod;
    }

    return (bool) existsInCollectionMethod.Invoke(collection.CurrentValue, new[]
    {
      saved
    })!;
  }

  private bool HasChanges(EntityEntry entry, object source, object changed)
  {
    return entry.CurrentValues.Properties
      .Where(_ => !_.IsShadowProperty())
      .Any(prop =>
      {
        var getter = prop.GetGetter();
        return !Equals(getter.GetClrValue(source), getter.GetClrValue(changed));
      });
  }

  private static void AddToCollection(CollectionEntry collection, object saved)
  {
    var type = collection.CurrentValue!.GetType();
    if (!AddMethodsCache.TryGetValue(type, out var addToCollectionMethod))
    {
      addToCollectionMethod =
        type.GetMethod(nameof(ICollection<T>.Add)) ??
        throw new InvalidOperationException("Can't find method " + nameof(ICollection<T>.Add));
      AddMethodsCache[type] = addToCollectionMethod;
    }

    addToCollectionMethod.Invoke(collection.CurrentValue, new[]
    {
      saved
    });
  }

  protected virtual void MarkRemoved(T aggregate)
  {
    Context.Set<T>().RemoveRange(aggregate);
  }

  protected virtual void MarkRemoved(IEnumerable<T> aggregates)
  {
    Context.Set<T>().RemoveRange(aggregates);
  }
}