using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation;

// ReSharper disable once InconsistentNaming
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddApplicationDbContext<T>(this IServiceCollection services, IHostEnvironment env)
    where T : DbContext
  {
    var migrationsAssemblyName = typeof(T).Assembly.GetName().Name;

    services.AddScoped<DbContext>(svc => svc.GetRequiredService<T>());

    services.AddDbContextPool<T>((svc, configurer) =>
    {
      var cfg = svc.GetRequiredService<IConfiguration>();
      configurer.UseNpgsql(cfg.GetConnectionString("Npgsql"), o =>
        {
          o.MigrationsAssembly(migrationsAssemblyName)
            /*.EnableRetryOnFailure(cfg.MaxRetryCount)*/
            .UseNodaTime();
        })
        .EnableDetailedErrors(env.IsDevelopment())
        .UseLoggerFactory(svc.GetRequiredService<ILoggerFactory>());
    });

    return services;
  }
}