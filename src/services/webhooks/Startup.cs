using Centurion.Contracts.Checkout.Integration;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Composition;
using Centurion.SeedWork.Web.Composition;
using Centurion.SeedWork.Web.Foundation;
using Centurion.TaskManager.Infrastructure.Config;
using Centurion.WebhookSender.Composition;
using Centurion.WebhookSender.Core;
using Centurion.WebhookSender.Infrastructure;
using Centurion.WebhookSender.Web.Grpc;
using LightInject;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Centurion.WebhookSender;

public class Startup
{
  private IServiceContainer _container = null!;
  private readonly IConfiguration _configuration;
  private readonly IHostEnvironment _hostEnvironment;

  public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
  {
    _configuration = configuration;
    _hostEnvironment = hostEnvironment;
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services
      .InitializeConfiguration(_configuration)
      .AddConfiguredAuthentication(_configuration)
      .AddConfiguredAuthorization(_configuration);

    TelemetryRegistrationServiceCollection.AddConfiguredTelemetry(services);
    services.AddGrpc();
    services.AddHttpContextAccessor();
    services.AddMassTransit(bc =>
      {
        bc.UsingRabbitMq((regCtx, cfg) =>
        {
          cfg.Host(_configuration.GetConnectionString("RabbitMq"));

          cfg.ConfigureProtobufSerializerFor(ProductCheckedOut.Parser);

          var integCfg = regCtx.GetRequiredService<IntegrationBusConfig>();
          cfg.ConfigureIntegrationEvents(integCfg)
            .AddTopicConsumer<SendNotificationOnProductCheckoutOut, ProductCheckedOut>(integCfg, regCtx);
        });
      })
      .AddMassTransitHostedService()
      .AddScoped<SendNotificationOnProductCheckoutOut>();

    services.AddDbContextPool<WebhooksContext>(o =>
    {
      var migrationsAssemblyName = typeof(WebhooksContext).Assembly.GetName().Name;
      o.UseNpgsql(_configuration.GetConnectionString("Npgsql"), c => c.MigrationsAssembly(migrationsAssemblyName))
        .EnableDetailedErrors(_hostEnvironment.IsDevelopment());
    });

    services.AddScoped<DbContext>(s => s.GetRequiredService<WebhooksContext>());

    services.AddHttpClient<IDiscordClient, DiscordClient>();
  }

  public void ConfigureContainer(IServiceContainer container)
  {
    container.RegisterFrom<ApplicationModule>();
    container.RegisterFrom<WebSeedWorkModule>();
    container.RegisterFrom<InfraSeedWorkModule>();

    _container = container;
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    _container.Compile();
    DiExtensions.UseConfiguredElasticApm(app, _configuration);

    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }

    DiExtensions.UseCommonHttpBehavior(app, env);
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints => { endpoints.MapGrpcService<WebhookService>(); });
  }
}