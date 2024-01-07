using Centurion.SeedWork.Infra.EfCoreNpgsql;
using Microsoft.EntityFrameworkCore;

namespace Centurion.CloudManager.Infra;

public class CloudContext : DbContext
{
  public CloudContext(DbContextOptions<CloudContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    modelBuilder.UseSnakeCaseNamingConvention();
    base.OnModelCreating(modelBuilder);
  }
}