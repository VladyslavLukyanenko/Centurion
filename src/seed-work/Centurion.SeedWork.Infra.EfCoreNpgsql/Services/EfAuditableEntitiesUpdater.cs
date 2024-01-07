using Centurion.SeedWork.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NodaTime;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Services;

public class EfAuditableEntitiesUpdater<TKey> : IEfAuditableEntitiesUpdater
{
  private readonly IClock _clock;
  private readonly IIdentityProvider _identityProvider;

  public EfAuditableEntitiesUpdater(IClock clock, IIdentityProvider identityProvider)
  {
    _clock = clock;
    _identityProvider = identityProvider;
  }

  public void Update(DbContext context)
  {
    var entries = context.ChangeTracker
      .Entries()
      .Where(_ => _.Metadata.ClrType.IsEntity());

    foreach (EntityEntry entry in entries)
    {
      switch (entry.State)
      {
        case EntityState.Modified:
          if (entry.Entity is ITimestampAuditable)
          {
            entry.CurrentValues[nameof(ITimestampAuditable.UpdatedAt)] = _clock.GetCurrentInstant();
          }

          if (entry.Entity is IAuthorAuditable<TKey>)
          {
            entry.CurrentValues[nameof(IAuthorAuditable<TKey>.UpdatedBy)] = _identityProvider.GetCurrentIdentity();
          }
          break;
        case EntityState.Added:
          if (entry.Entity is ITimestampAuditable)
          {
            entry.CurrentValues[nameof(ITimestampAuditable.UpdatedAt)] = _clock.GetCurrentInstant();
            entry.CurrentValues[nameof(ITimestampAuditable.CreatedAt)] = _clock.GetCurrentInstant();
          }

          if (entry.Entity is IAuthorAuditable<TKey>)
          {
            entry.CurrentValues[nameof(IAuthorAuditable<TKey>.UpdatedBy)] = _identityProvider.GetCurrentIdentity();
            entry.CurrentValues[nameof(IAuthorAuditable<TKey>.CreatedBy)] = _identityProvider.GetCurrentIdentity();
          }

          break;
      }

      if (ReflectionHelper.IsGenericAssignableFrom(entry.Metadata.ClrType, typeof(IConcurrentEntity)))
      {
        entry.CurrentValues[nameof(IConcurrentEntity.ConcurrencyStamp)] = Guid.NewGuid().ToString("N");
      }
    }
  }
}
