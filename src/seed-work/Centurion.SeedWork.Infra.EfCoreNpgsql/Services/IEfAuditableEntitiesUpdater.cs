using Microsoft.EntityFrameworkCore;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Services;

public interface IEfAuditableEntitiesUpdater
{
  void Update(DbContext context);
}
