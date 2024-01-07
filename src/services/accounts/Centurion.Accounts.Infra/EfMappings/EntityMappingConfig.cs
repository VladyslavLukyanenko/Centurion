using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.ValueGenerators;

namespace Centurion.Accounts.Infra.EfMappings;

public abstract class EntityMappingConfig<T>
  : IEntityTypeConfiguration<T>
  where T : class
{
  protected bool MappedToSeparateTable = true;

  protected abstract string SchemaName { get; }
  protected bool SkipDashboardRelationshipConfiguration = false;

  protected static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
  {
    //            ContractResolver = new 
    NullValueHandling = NullValueHandling.Ignore,
    ContractResolver = new CamelCasePropertyNamesContractResolver()
  }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

  public virtual void Configure(EntityTypeBuilder<T> builder)
  {
    if (MappedToSeparateTable)
    {
      var tableName = typeof(T).Name;
      builder.ToTable(tableName, SchemaName);
    }

    if (MappedToSeparateTable && ReflectionHelper.IsGenericAssignableFrom(typeof(T), typeof(IEntity<>)))
    {
      SetupIdGenerationStrategy(builder);
    }

    if (!SkipDashboardRelationshipConfiguration && typeof(IDashboardBoundEntity).IsAssignableFrom(typeof(T)))
    {
      builder.HasOne<Dashboard>()
        .WithMany()
        .HasForeignKey(nameof(IDashboardBoundEntity.DashboardId));
    }

    if (typeof(ISoftRemovable).IsAssignableFrom(typeof(T)))
    {
      builder.HasIndex(nameof(ISoftRemovable.RemovedAt));
    }

    if (typeof(IAuthorAuditable).IsAssignableFrom(typeof(T)))
    {
      builder.HasOne<User>()
        .WithMany()
        .HasForeignKey(nameof(IAuthorAuditable.CreatedBy))
        .IsRequired(false);

      builder.HasOne<User>()
        .WithMany()
        .HasForeignKey(nameof(IAuthorAuditable.UpdatedBy))
        .IsRequired(false);

      builder.Property(nameof(IAuthorAuditable.CreatedBy))
        .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

      builder.Property(nameof(IAuthorAuditable.UpdatedBy));
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

  protected virtual void SetupIdGenerationStrategy(EntityTypeBuilder<T> builder)
  {
    builder.Property(nameof(IEntity<int>.Id))
      .UseHiLo((typeof(T).Name + "HiLoSequence").ToSnakeCase(), SchemaName.ToSnakeCase());
  }

  protected string ResolveNavigationField<TSource, TNav>(string? possibleName = null)
  {
    return EntityMappingUtils.ResolveNavigationField<TSource, TNav>(possibleName);
  }

  protected string ResolveNavigationField<TNav>(string? possibleName = null)
  {
    return EntityMappingUtils.ResolveNavigationField<T, TNav>(possibleName);
  }

  protected void MappedToTableWithDefaults<TEntity, TDependentEntity>(
    OwnedNavigationBuilder<TEntity, TDependentEntity> builder)
    where TEntity : class
    where TDependentEntity : class
  {
    builder.ToTable(typeof(TDependentEntity).Name, SchemaName);
  }

  protected string ToJson(object? value)
  {
    return JsonConvert.SerializeObject(value, JsonSettings);
  }

  [return: MaybeNull]
  protected TResult FromJson<TResult>(string json)
  {
    return JsonConvert.DeserializeObject<TResult>(json, JsonSettings);
  }
}