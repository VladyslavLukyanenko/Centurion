using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Sockets;
using AspNetCoreRateLimit;
using Centurion.Accounts.Foundation.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Centurion.Accounts.Infra;

namespace Centurion.Accounts.Foundation;

// ReSharper disable once InconsistentNaming
public static class IWebHostExtensions
{
  public static async Task<IHost> MigrateDbContextAsync<TContext>(this IHost webHost,
    Func<TContext, IServiceProvider, Task> seeder)
    where TContext : DbContext
  {
    using (var scope = webHost.Services.CreateScope())
    {
      var services = scope.ServiceProvider;
      var efConfig = services.GetRequiredService<IOptions<EfCoreConfig>>();
      var logger = services.GetRequiredService<ILogger<TContext>>();
      if (!efConfig.Value.MigrateDatabaseOnStart)
      {
        logger.LogInformation($"Skipping migration database associated with context {typeof(TContext).Name}");
        return webHost;
      }

      var context = services.GetRequiredService<TContext>();

      try
      {
        logger.LogInformation("Checking database accessibility.");
        var retryPolicy = Policy.Handle<SocketException>()
          .Or<DbException>()
          .WaitAndRetryAsync(10,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (_, retryAttempt, _) =>
            {
              logger.LogError("Failed to connect to database. Next retry in {RetryAttempt}",
                retryAttempt);
            });

        await retryPolicy.ExecuteAsync(async () =>
        {
          var conn = context.Database.GetDbConnection();
          await conn.OpenAsync();
          conn.Close();
        });

        logger.LogInformation(
          $"Database is accessible. Migrating database associated with context {typeof(TContext).Name}");

        var retry = Policy.Handle<SqlException>()
          .WaitAndRetryAsync(new[]
          {
            TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(8)
          });

        await retry.ExecuteAsync(async () =>
        {
          //if the sql server container is not created on run docker compose this
          //migration can't fail for network related exception. The retry options for DbContext only 
          //apply to transient exceptions.

          await context.Database.MigrateAsync();
          await seeder(context, services);
        });


        logger.LogInformation($"Migrated database associated with context {typeof(TContext).Name}");
      }
      catch (Exception ex)
      {
        logger.LogError(ex,
          $"An error occurred while migrating the database used on context {typeof(TContext).Name}");
      }
    }

    return webHost;
  }

  public static async ValueTask SeedRateLimiterPolicyStoreAsync(this IHost host)
  {
    using var scope = host.Services.CreateScope();
    var clientPolicyStore = scope.ServiceProvider.GetRequiredService<IClientPolicyStore>();
    await clientPolicyStore.SeedAsync();
  }

  public static async ValueTask SetupQuartzStoreAsync(this IHost host, CancellationToken ct = default)
  {
    using var scope = host.Services.CreateScope();
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var createTablesSqlPath = cfg["Quartz:TablesCreateScriptPath"];
    var checkIfTablesExistsSql = cfg["Quartz:TablesExistsScript"];
    var dbCtx = scope.ServiceProvider.GetRequiredService<DbContext>();

    var connection = dbCtx.Database.GetDbConnection();
    await connection.OpenAsync(ct);
    await using var tablesExistsCmd = connection.CreateCommand();
    tablesExistsCmd.CommandText = checkIfTablesExistsSql;
    tablesExistsCmd.CommandType = CommandType.Text;
    var tablesExists = (bool?) await tablesExistsCmd.ExecuteScalarAsync(ct);
    if (tablesExists == false)
    {
      await using var createTablesCmd = connection.CreateCommand();
      createTablesCmd.CommandType = CommandType.Text;
      createTablesCmd.CommandText = await File.ReadAllTextAsync(createTablesSqlPath, ct);
      await createTablesCmd.ExecuteNonQueryAsync(ct);
    }
  }

  public static async ValueTask MigrateDatabaseIfAllowedAsync(this IHost webHost, CancellationToken ct = default)
  {
    await webHost.MigrateDbContextAsync<DbContext>(async (context, services) =>
    {
      IEnumerable<IDataSeeder> seeders = services.GetServices<IDataSeeder>().OrderBy(_ => _.Order);
      await using (await context.Database.BeginTransactionAsync(ct))
      {
        foreach (IDataSeeder seeder in seeders)
        {
          await seeder.SeedAsync();
        }

        await context.Database.CommitTransactionAsync(ct);
      }
    });
  }
}