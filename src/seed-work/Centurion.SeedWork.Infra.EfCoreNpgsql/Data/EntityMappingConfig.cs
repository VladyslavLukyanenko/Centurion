using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Centurion.SeedWork.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Data;

public abstract class EntityMappingConfig<T>
  : IEntityTypeConfiguration<T>
  where T : class
{
  protected bool MappedToSeparateTable = true;

  // ReSharper disable once StaticMemberInGenericType
  private static readonly ISet<Type> HiLoSupportedTypes = new HashSet<Type>
  {
    typeof(short),
    typeof(int),
    typeof(long),
  };

  private static readonly ISet<Type> SeqGuidSupportedTypes = new HashSet<Type>
  {
    typeof(Guid)
  };

  protected abstract string SchemaName { get; }

  protected ValueComparer CreateListComparer<TItem>() => new ValueComparer<IList<TItem>>(
    (c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, SafeGetHashCode(v))),
    c => c.ToList());

  protected ValueComparer CreateSetComparer<TItem>() => new ValueComparer<HashSet<TItem>>(
    (c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, SafeGetHashCode(v))),
    c => c.ToHashSet());

  protected ValueComparer CreateDictionaryComparer<TKey, TValue>() where TKey : notnull =>
    new ValueComparer<IDictionary<TKey, TValue>>(
      (c1, c2) => c1!.Count == c2!.Count && !c1.Except(c2).Any(),
      c => c.Aggregate(0, (a, v) => HashCode.Combine(a,
        HashCode.Combine(v.Key.GetHashCode(), SafeGetHashCode(v.Value)))
      ),
      c => new Dictionary<TKey, TValue>(c));

  private static int SafeGetHashCode<TItem>(TItem v)
  {
    return Equals(v, default) ? 0 : v.GetHashCode();
  }

  // protected static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
  // {
  //   //            ContractResolver = new 
  //   NullValueHandling = NullValueHandling.Ignore,
  //   ContractResolver = new CamelCasePropertyNamesContractResolver()
  // }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

  public virtual void Configure(EntityTypeBuilder<T> builder)
  {
    if (MappedToSeparateTable)
    {
      var tableName = typeof(T).Name;
      builder.ToTable(tableName, SchemaName);
    }

    if (MappedToSeparateTable && ReflectionHelper.IsGenericAssignableFrom(typeof(T), typeof(IEntity<>)))
    {
      var idProperty = typeof(T).GetProperty(nameof(IEntity<int>.Id))!;
      if (HiLoSupportedTypes.Contains(idProperty.PropertyType))
      {
        SetupHiLoIdGenerationStrategy(builder);
      }
      else if (SeqGuidSupportedTypes.Contains(idProperty.PropertyType))
      {
        SetupSeqGuidGenerationStrategy(builder);
      }
    }

//       
//       if (typeof(IStoreBoundEntity).IsAssignableFrom(typeof(T)))
//       {
//         builder.HasOne<Store>()
//           .WithMany()
//           .HasForeignKey(nameof(IStoreBoundEntity.StoreId));
//       }
//
    if (typeof(ISoftRemovable).IsAssignableFrom(typeof(T)))
    {
      builder.HasIndex(nameof(ISoftRemovable.RemovedAt));
    }

    if (typeof(IAuthorAuditable<string>).IsAssignableFrom(typeof(T)))
    {
      /*
               builder.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(nameof(IAuthorAuditable.CreatedBy))
                 .IsRequired(false);
      
               builder.HasOne<ApplicationUser>()
                 .WithMany()
                 .HasForeignKey(nameof(IAuthorAuditable.UpdatedBy))
                 .IsRequired(false);*/

      builder.Property(nameof(IAuthorAuditable<string>.CreatedBy))
        .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

      builder.Property(nameof(IAuthorAuditable<string>.UpdatedBy));
    }

    if (typeof(ITimestampAuditable).IsAssignableFrom(typeof(T)))
    {
      builder.Property(nameof(ITimestampAuditable.CreatedAt))
        //.ValueGeneratedOnAdd()
        .IsRequired()
        .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

      builder.Property(nameof(ITimestampAuditable.UpdatedAt)).IsRequired();
    }

    if (typeof(IEventSource).IsAssignableFrom(typeof(T)))
    {
      builder.Ignore(_ => ((IEventSource) _).DomainEvents);
    }

    if (typeof(IConcurrentEntity).IsAssignableFrom(typeof(T)))
    {
      var propertyBuilder = builder.Property(nameof(IConcurrentEntity.ConcurrencyStamp));
      propertyBuilder
        .ValueGeneratedOnAddOrUpdate()
        .HasValueGenerator<ConcurrencyStampValueGenerator>()
        .IsRequired()
        .IsConcurrencyToken();
//
//                propertyBuilder
//                    .Metadata.ValueGenerated = ValueGenerated.OnAddOrUpdate;
      propertyBuilder
        .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

      propertyBuilder
        .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
    }
  }

  protected virtual void SetupHiLoIdGenerationStrategy(EntityTypeBuilder<T> builder)
  {
    builder.Property(nameof(IEntity<int>.Id))
      .UseHiLo((typeof(T).Name + "HiLoSequence").ToSnakeCase(), SchemaName.ToSnakeCase());
  }
  protected virtual void SetupSeqGuidGenerationStrategy(EntityTypeBuilder<T> builder)
  {
    builder.Property(nameof(IEntity<int>.Id))
      .HasValueGenerator<SequentialGuidValueGenerator>();
  }

  //
  // protected string ResolveNavigationField<TSource, TNav>(string? possibleName = null)
  // {
  //   return EntityMappingUtils.ResolveNavigationField<TSource, TNav>(possibleName);
  // }
  //
  // protected string ResolveNavigationField<TNav>(string? possibleName = null)
  // {
  //   return EntityMappingUtils.ResolveNavigationField<T, TNav>(possibleName);
  // }
  //
  // protected void MappedToTableWithDefaults<TEntity, TDependentEntity>(
  //   OwnedNavigationBuilder<TEntity, TDependentEntity> builder)
  //   where TEntity : class
  //   where TDependentEntity : class
  // {
  //   builder.ToTable(typeof(TDependentEntity).Name, SchemaName);
  // }
  //
  protected string ToJson(object? value)
  {
    return JsonSerializer.Serialize(value /*, JsonSettings*/);
  }

  [return: MaybeNull]
  protected TResult FromJson<TResult>(string json)
  {
    return JsonSerializer.Deserialize<TResult>(json /*, JsonSettings*/);
  }
}