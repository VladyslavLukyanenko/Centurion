using System.Diagnostics;
using Autofac;
using Centurion.Cli.Core;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Elasticsearch;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;

namespace Centurion.Cli.Composition;

public class LoggerModule : Module
{
  private readonly string _environment;

  public LoggerModule(string environment)
  {
    _environment = environment;
  }

  protected override void Load(ContainerBuilder builder)
  {
    var logsStore = Path.Combine(AppInfo.StorageLocation, "Logs", "{Date}.json");
    builder.Register(ctx =>
      {
        var config = ctx.Resolve<IConfiguration>();
        var logConfig = new LoggerConfiguration();
        if (AppInfo.IsProduction)
        {
          logConfig.MinimumLevel.Warning();
        }
        else
        {
          logConfig.MinimumLevel.Debug()
            .MinimumLevel.Override("Elastic", LogEventLevel.Warning)
            .MinimumLevel.Override("Centurion", LogEventLevel.Debug);
        }

        logConfig.Enrich.FromLogContext()
          .Enrich.With(new UserInfoEnricher(ctx.Resolve<Lazy<IIdentityService>>(),
            ctx.Resolve<Lazy<ILicenseKeyProvider>>()))
          .Enrich.WithProperty("environment", _environment)
          .Enrich.WithMachineName()
          .Enrich.WithThreadId()
          .Enrich.WithEnvironmentUserName();

        const string logFormat = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Exception}{NewLine}";
        logConfig = logConfig.WriteTo.ColoredConsole(outputTemplate: logFormat);

        Log.Logger = logConfig
          .WriteTo.Debug(outputTemplate: logFormat)
          .WriteTo.Async(lcfg =>
          {
            // todo: set path
            lcfg.RollingFile(new JsonFormatter(), logsStore, fileSizeLimitBytes: 104_857_600,
              retainedFileCountLimit: null);
          })
          .ReadFrom.Configuration(ctx.Resolve<IConfiguration>())
          .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(config["ElasticSearch:ServerUrls"]))
          {
            IndexFormat = "cli-client-{0:yyyyMMdd}",
            AutoRegisterTemplate = true,
            CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage: true, inlineFields: true),
            EmitEventFailure =
              EmitEventFailureHandling.WriteToSelfLog |
              EmitEventFailureHandling.RaiseCallback |
              EmitEventFailureHandling.ThrowException,
            FailureCallback = e => { Debug.WriteLine("Unable to submit event " + e.MessageTemplate); }
          })
          .CreateLogger();

        return new SerilogLoggerProvider();
      })
      .SingleInstance();

    builder
      .Register(_ => new LoggerFactory(new ILoggerProvider[] { _.Resolve<SerilogLoggerProvider>() }))
      .As<ILoggerFactory>()
      .SingleInstance();

    builder.RegisterGeneric(typeof(Logger<>))
      .As(typeof(ILogger<>))
      .SingleInstance();
  }


  private class UserInfoEnricher : ILogEventEnricher
  {
    private readonly Lazy<IIdentityService> _idSrv;
    private readonly Lazy<ILicenseKeyProvider> _lsvc;

    public UserInfoEnricher(Lazy<IIdentityService> idSrv, Lazy<ILicenseKeyProvider> lsvc)
    {
      _idSrv = idSrv;
      _lsvc = lsvc;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
      User? currentUser = _idSrv.Value.CurrentUser;
      string? licenseKey = _lsvc.Value.CurrentLicenseKey;
      logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("licenseKey", licenseKey));
      logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("user", currentUser, true));
    }
  }
}