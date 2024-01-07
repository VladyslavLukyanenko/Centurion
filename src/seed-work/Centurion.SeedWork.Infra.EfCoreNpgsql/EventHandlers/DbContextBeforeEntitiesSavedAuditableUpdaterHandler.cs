using Centurion.SeedWork.Infra.EfCoreNpgsql.Events;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Services;
using MediatR;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.EventHandlers;

public class DbContextBeforeEntitiesSavedAuditableUpdaterHandler : NotificationHandler<DbContextBeforeEntitiesSaved>
{
  private readonly IEfAuditableEntitiesUpdater _auditableEntitiesUpdater;

  public DbContextBeforeEntitiesSavedAuditableUpdaterHandler(IEfAuditableEntitiesUpdater auditableEntitiesUpdater)
  {
    _auditableEntitiesUpdater = auditableEntitiesUpdater;
  }

  protected override void Handle(DbContextBeforeEntitiesSaved @event)
  {
    _auditableEntitiesUpdater.Update(@event.Context);
  }
}
