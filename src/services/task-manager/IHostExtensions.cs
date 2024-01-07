using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Services;
using Microsoft.EntityFrameworkCore;

namespace Centurion.TaskManager;

// ReSharper disable once InconsistentNaming
public static class IHostExtensions
{
  public static async Task MigrateDatabaseIfAllowedAsync(this IHost webHost)
  {
    await webHost.MigrateDbContextAsync<DbContext>(async (context, services) =>
    {
      IEnumerable<IDataSeeder> seeders = services.GetServices<IDataSeeder>().OrderBy(_ => _.Order);
      await using (await context.Database.BeginTransactionAsync())
      {
        foreach (IDataSeeder seeder in seeders)
        {
          await seeder.SeedAsync();
        }

        await context.Database.CommitTransactionAsync();
      }
    });
  }
}