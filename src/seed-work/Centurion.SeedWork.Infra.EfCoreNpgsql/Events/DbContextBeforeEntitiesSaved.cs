using Microsoft.EntityFrameworkCore;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Events;

public class DbContextBeforeEntitiesSaved : InfraEvent
{
  public DbContextBeforeEntitiesSaved(DbContext context)
  {
    Context = context;
  }
    
  public DbContext Context { get; }
}