using Autofac;
using Centurion.Cli.Core;
using Centurion.Cli.Core.Config;
using Microsoft.Extensions.Configuration;

namespace Centurion.Cli.Composition;

public class ConfigModule : Module
{
  private readonly string _environment;

  public ConfigModule(string environment)
  {
    _environment = environment;
  }

  protected override void Load(ContainerBuilder container)
  {
    var inMemoryConfig = new Dictionary<string, string>
    {
      {"ElasticSearch:ServerUrls", "https://clientlogger:mc123ad9275287kwFAFfmp121614@observability.centurion.gg"},
      {"ElasticApm:ServerUrls", "http://apm.centurion.gg"},
      {"ElasticApm:TransactionIgnoreUrls", "/hubs/*"},
      {"ElasticApm:TransactionSampleRate", "0.2"},
      {"ElasticApm:Environment", _environment},
      {"ElasticApm:ServiceName", AppInfo.ProductTechName},
      {"ElasticApm:CloudProvider", "none"},

      {"Security:ReauthenticateInterval", "0:01:00"},

      {"Clients:TaskManagerUrl", "https://taskmanager-api.centurion.gg/"},
      {"Clients:NotificationsUrl", "https://taskmanager-api.centurion.gg/"},
      {"Clients:WebhookServiceUrl", "https://webhooks-api.centurion.gg/"},
      {"Clients:AccountsUrl", "https://accounts-api.centurion.gg/"},

      {"Harvesters:ChromeDistroArchiveLocation", Path.Combine(AppInfo.InstallationPath, "external", "chrome.zip")},

      {"General:CheckoutSoundFilePath", Path.Combine(AppInfo.InstallationPath, "Assets", "Sounds", "Checkout.mp3")},
      {"General:DeclineSoundFilePath", Path.Combine(AppInfo.InstallationPath, "Assets", "Sounds", "Decline.mp3")},
    };

#if DEBUG
    inMemoryConfig["ElasticSearch:ServerUrls"] = "http://elastic:NotASecretPassword@localhost:9200";
    inMemoryConfig["ElasticApm:ServerUrls"] = "http://localhost:8200";
    inMemoryConfig["ElasticApm:TransactionSampleRate"] = "1";
    //
    inMemoryConfig["Clients:TaskManagerUrl"] = "https://localhost:5003";
    inMemoryConfig["Clients:NotificationsUrl"] = "http://localhost:5002";
    inMemoryConfig["Clients:WebhookServiceUrl"] = "https://localhost:5009";
    inMemoryConfig["Clients:AccountsUrl"] = "https://localhost:5001";

    inMemoryConfig["Harvesters:ChromeDistroArchiveLocation"] = "D:/Workspace/CenturionLabs/centurion-3rdparty-deps/chrome.zip";
#endif

    var builder = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddInMemoryCollection(inMemoryConfig)
      .AddJsonFile("appsettings.json", true, true)
      .AddJsonFile("serilogsettings.json", true, true)
      .AddJsonFile($"appsettings.{_environment}.json", true)
      .AddJsonFile("appsettings.local.json", true)
      .AddEnvironmentVariables();

    var args = Environment.GetCommandLineArgs();
    if (args.Any())
    {
      builder.AddCommandLine(args);
    }

    var config = builder.Build();
    container.RegisterInstance(config)
      .SingleInstance();

    container.RegisterInstance(config)
      .As<IConfiguration>()
      .SingleInstance();


    var storageLocation = Path.Combine(EnvironmentHelper.GetLocalAppDataPath(), AppInfo.ProductTechName);
    var cfg = new ApplicationConfig
    {
      StorageLocation = storageLocation,
      ConnectionStrings =
      {
        LiteDb = Path.Combine(storageLocation, $"{AppInfo.ProductTechName}.db")
      }
    };

    config.GetSection("Clients").Bind(cfg.ClientsConfig);
    config.GetSection("Security").Bind(cfg.Security);
    config.GetSection("Harvesters").Bind(cfg.HarvesterConfig);
    config.GetSection("General").Bind(cfg.General);

    container.RegisterInstance(cfg);
    container.RegisterInstance(cfg.Security);
    container.RegisterInstance(cfg.ClientsConfig);
    container.RegisterInstance(cfg.ConnectionStrings);
    container.RegisterInstance(cfg.HarvesterConfig);
    container.RegisterInstance(cfg.General);
  }
}