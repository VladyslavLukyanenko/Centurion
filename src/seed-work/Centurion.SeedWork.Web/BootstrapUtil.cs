using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Elastic.Apm.SerilogEnricher;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Centurion.SeedWork.Web;

public static class BootstrapUtil
{
  public const string ProductionEnv = "Production";
  private static string? _deploymentEnvironment;

  public static IConfigurationRoot Configure()
  {
    _deploymentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? ProductionEnv;

    var location = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
    var builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile(Path.Combine(location!, "serilogsettings.json"), false, true)
      .AddJsonFile(Path.Combine(location!, $"serilogsettings.{_deploymentEnvironment}.json"), true, true)
      .AddJsonFile("appsettings.json", false, true)
      .AddJsonFile($"appsettings.{_deploymentEnvironment}.json", true)
      .AddJsonFile("appsettings.local.json", true)
      .AddEnvironmentVariables();

    var args = Environment.GetCommandLineArgs();
    if (args.Any())
    {
      builder.AddCommandLine(args);
    }

    var appConfig = builder.Build();

    Log.Logger = new LoggerConfiguration()
      .Enrich.WithElasticApmCorrelationInfo()
      .ReadFrom.Configuration(appConfig)
      .CreateLogger();

    return appConfig;
  }

  public static async Task StartWebHostAsync(string[] args, Func<string[], IHostBuilder> builderFactory,
    Func<IHost, ValueTask>? hostInitializer = null)
  {
    var logger = Log.ForContext(typeof(BootstrapUtil));
    try
    {
      logger.Information("[{Environment}] Starting application {ProductName}@{DevelopmentTeam} {InformationalVersion}",
        _deploymentEnvironment, AppConstants.ProductName, AppConstants.DevelopmentTeam,
        AppConstants.InformationalVersion);

      var webHost = builderFactory(args).Build();

      if (hostInitializer != null)
      {
        await hostInitializer(webHost);
      }

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

  public static IHostBuilder CreateHostBuilder<TStartup>(string[] args, IConfiguration configuration)
    where TStartup : class
  {
    return Host.CreateDefaultBuilder(args)
      .ConfigureHostConfiguration(c => c.AddConfiguration(configuration))
      .ConfigureAppConfiguration(c => c.AddConfiguration(configuration))
      .ConfigureWebHostDefaults(configure =>
      {
        configure
          .ConfigureKestrel(o =>
          {
            o.Limits.MaxRequestBodySize = 300 * (int)Math.Pow(1024, 2);
            var cfg = configuration.GetSection("ServerBindings").Get<ServerBindingsConfig>();
            o.Listen(IPAddress.Any, cfg.Http1Port,
              lo => { lo.Protocols = cfg.Http2Port.HasValue ? HttpProtocols.Http1AndHttp2 : HttpProtocols.Http2; });
            if (!cfg.Http2Port.HasValue)
            {
              return;
            }

            o.Listen(IPAddress.Any, cfg.Http2Port.Value, lo =>
            {
              lo.Protocols = HttpProtocols.Http2;
              if (cfg.CleartextOn2Port)
              {
                return;
              }

              var useDefaultRaw = configuration["Cert:UseDefault"];
              if (!string.IsNullOrEmpty(useDefaultRaw) && bool.TryParse(useDefaultRaw, out var useDefault)
                                                       && useDefault)
              {
                lo.UseHttps();
                return;
              }

              lo.UseHttps(adapterOptions =>
              {
                var pemFilePath = configuration["Cert:Pem"];
                if (!File.Exists(pemFilePath))
                {
                  throw new InvalidOperationException("Can't find pem: " + pemFilePath);
                }

                var keyPemFilePath = configuration["Cert:Key"];
                if (!File.Exists(keyPemFilePath))
                {
                  throw new InvalidOperationException("Can't find key: " + keyPemFilePath);
                }

                adapterOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(pemFilePath, keyPemFilePath);
              });
            });
          })
          .UseStartup<TStartup>();
      })
      .ConfigureLogging((_, logging) => { logging.AddDefaultLoggingProvider(configuration); })
      .UseConsoleLifetime()
      .UseLightInject();
  }
}