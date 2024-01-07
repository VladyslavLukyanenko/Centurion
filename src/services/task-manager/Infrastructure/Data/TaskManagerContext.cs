using Centurion.SeedWork.Infra.EfCoreNpgsql;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager.Infrastructure.Data;

public class TaskManagerContext : DbContext
{
  public TaskManagerContext(DbContextOptions<TaskManagerContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    base.OnModelCreating(modelBuilder);
    modelBuilder.UseSnakeCaseNamingConvention();
  }
}