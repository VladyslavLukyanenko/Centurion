using Autofac.Extensions.DependencyInjection;
using Centurion.Accounts.Foundation;
using Elastic.Apm.SerilogEnricher;
using Microsoft.IdentityModel.Logging;
using Serilog;

namespace Centurion.Accounts;

public class Program
{
  private const string ProductionEnv = "Production";

  static Program()
  {
    // todo: remove it before going to prod
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    IdentityModelEventSource.ShowPII = true;
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ProductionEnv;

    var builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("serilogsettings.json", false, true)
      .AddJsonFile("appsettings.json", false, true)
      .AddJsonFile($"appsettings.{environmentName}.json", true)
      .AddJsonFile("appsettings.local.json", true)
      .AddEnvironmentVariables();

    var args = Environment.GetCommandLineArgs();
    if (args.Any())
    {
      builder.AddCommandLine(args);
    }

    Configuration = builder.Build();
    var loggerConfiguration = new LoggerConfiguration()
      .Enrich.WithElasticApmCorrelationInfo()
      .ReadFrom.Configuration(Configuration);

    Log.Logger = loggerConfiguration.CreateLogger();
  }

  private static IConfigurationRoot Configuration { get; }

  public static async Task Main(string[] args)
  {
    var logger = Log.ForContext(typeof(Program));
    try
    {
      logger.Information("Starting application");

      var webHost = CreateHostBuilder(args).Build();

      await webHost.MigrateDatabaseIfAllowedAsync();
      await webHost.SeedRateLimiterPolicyStoreAsync();
      await webHost.SetupQuartzStoreAsync();

      await webHost.RunAsync();
    }
    catch (Exception e)
    {
      logger.Fatal(e, "Host terminated unexpectedly");
     throw;
    }
    finally
    {
      Log.CloseAndFlush();
    }
  }

  private static IHostBuilder CreateHostBuilder(string[] args)
  {
    return Host.CreateDefaultBuilder(args)
      .UseServiceProviderFactory(new AutofacServiceProviderFactory())
      .ConfigureHostConfiguration(c => c.AddConfiguration(Configuration))
      .ConfigureAppConfiguration(c => c.AddConfiguration(Configuration))
      .ConfigureWebHostDefaults(configure =>
      {
        configure
          .ConfigureServices(container => { container.AddAutofac(); })
          .UseStartup<Startup>();
      })
      .ConfigureLogging((ctx, logging) =>
      {
        logging.ClearProviders();
        logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));
        logging.AddSerilog();
      })
      .UseConsoleLifetime();
  }
}