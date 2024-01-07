using Centurion.Contracts.Checkout.Integration;
using Centurion.Contracts.CloudManager;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Composition;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation;
using Centurion.SeedWork.Infra.EfCoreNpgsql.NewtonsoftJson;
using Centurion.SeedWork.Web;
using Centurion.SeedWork.Web.Composition;
using Centurion.SeedWork.Web.Foundation;
using Centurion.TaskManager.Composition;
using Centurion.TaskManager.Core;
using Centurion.TaskManager.Infrastructure.Config;
using Centurion.TaskManager.Infrastructure.Data;
using Centurion.TaskManager.Infrastructure.MassTransit;
using Centurion.TaskManager.Web.Foundation;
using Centurion.TaskManager.Web.Grpc;
using Centurion.TaskManager.Web.Hubs;
using Centurion.TaskManager.Web.Services;
using Centurion.WebhookSender;
using LightInject;
using MassTransit;
using MessagePack;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NodaTime;

namespace Centurion.TaskManager;

public class Startup
{
  private readonly IConfiguration _configuration;
  private readonly IHostEnvironment _environment;
  private IServiceContainer _container = null!;

  public Startup(IConfiguration configuration, IHostEnvironment environment)
  {
    _configuration = configuration;
    _environment = environment;
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services.InitializeConfiguration(_configuration)
      .ConfigureCfgSectionAs<CloudServiceConfig>(_configuration, "CloudService")
      .AddConfiguredMvc()
      .AddConfiguredAuthentication(_configuration)
      .AddConfiguredAuthorization(_configuration)
      .AddApplicationDbContext<TaskManagerContext>(_environment)
      .AddConfiguredTelemetry()
      .AddHostedService<CloudManagerWorker>()
      .AddSignalR()
      .AddMessagePackProtocol(o => o.SerializerOptions = MessagePackSerializer.DefaultOptions)
      /*.AddHostedService<TaskActivityMonitorWorker>()*/;

    services.AddMassTransit(bc =>
      {
        bc.UsingRabbitMq((regCtx, cfg) =>
        {
          cfg.Host(_configuration.GetConnectionString("RabbitMq"));

          /*cfg.ConfigureProtobufSerializerFor(TaskStatusChanged.Parser);
          cfg.ConfigureProtobufSerializerFor(CheckoutStatusChanged.Parser);
          cfg.ConfigureProtobufSerializerFor(TerminateCheckoutRequested.Parser);*/
          cfg.ConfigureProtobufSerializerFor(ProductCheckedOut.Parser);

          var integCfg = regCtx.GetRequiredService<IntegrationBusConfig>();
          cfg.ConfigureIntegrationEvents(integCfg)
            .AddTopicConsumer<StoreEventOnProductCheckoutOut, ProductCheckedOut>(integCfg, regCtx);
        });
      })
      .AddScoped<StoreEventOnProductCheckoutOut>()
      .AddMassTransitHostedService();

    /*services.AddMarten(o =>
    {
      o.Connection(_configuration.GetConnectionString("Npgsql"));
      o.DatabaseSchemaName = "analytics";
      o.AutoCreateSchemaObjects = AutoCreate.All;
    });*/

    /*services.AddStackExchangeRedisExtensions<CustomMessagePackSerializer>(_ => new RedisConfiguration
      {
        ConnectionString = _configuration.GetConnectionString("Redis")
      })
      .AddScoped(_ => _.GetRequiredService<IRedisCacheClient>().Db0);*/

    services.Configure<JsonSerializerSettings>(o => NewtonsoftJsonSettingsFactory.ConfigureSettingsWithDefaults(o))
      .AddSingleton(s => s.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

    // services.AddHostedService<ProductFetcherWorker>();

    services.AddHttpClient();
    services.AddWebSockets(_ => { });

    services.AddGrpcClient<Cloud.CloudClient>(_ => _.Address = new Uri(_configuration["Services:CloudManagerUrl"]))
      .ConfigureChannel(c =>
      {
        c.HttpHandler = new SocketsHttpHandler
        {
          EnableMultipleHttp2Connections = true
        };

        c.MaxReceiveMessageSize = 1.Gb();
        c.MaxSendMessageSize = 1.Gb();
      });
  }

  public void ConfigureContainer(IServiceContainer container)
  {
    container.RegisterFrom<InfraSeedWorkModule>();
    container.RegisterFrom<AutomapperModule>();
    container.RegisterFrom<WebSeedWorkModule>();
    container.RegisterFrom<ApplicationModule>();

    var cfg = _configuration.GetSection("CloudService").Get<CloudServiceConfig>();
    if (cfg.UseSingleNode)
    {
      container.RegisterFrom<SingleNodeCloudModule>();
    }
    else
    {
      container.RegisterFrom<DistributedCloudModule>();
    }

    _container = container;
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    _container.Compile();
    app.Use((context, next) =>
    {
      // notice: all requests will be treated as trace root
      context.Request.Headers.Remove("traceparent");
      return next();
    });
    app.UseConfiguredElasticApm(_configuration);

    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
      // app.UseRedisInformation();
    }

    app.UseCommonHttpBehavior(env);

    app.UseStaticFiles();
    app.UseRouting();

    app.UseWebSockets();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
      endpoints.MapGet("/heartbeat", async context =>
        await context.Response.WriteAsync(SystemClock.Instance.GetCurrentInstant().ToString()));
      endpoints.MapGrpcService<OrchestratorService>();
      endpoints.MapGrpcService<ProductService>();
      endpoints.MapGrpcService<CheckoutTaskService>();
      endpoints.MapGrpcService<AnalyticsService>();
      endpoints.MapGrpcService<PresetsService>();

      endpoints.MapHub<TaskHub>("/hubs/task");
      endpoints.MapControllers();
      endpoints.MapRazorPages();
    });
  }
}