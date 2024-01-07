using System.Net.Mime;
using Centurion.CloudManager.App.Model;
using Centurion.CloudManager.Composition;
using Centurion.CloudManager.Infra;
using Centurion.CloudManager.Infra.Consumers;
using Centurion.CloudManager.Infra.RabbitMQ;
using Centurion.CloudManager.Infra.Services.Gcp;
using Centurion.CloudManager.Web;
using Centurion.CloudManager.Web.Foundation;
using Centurion.CloudManager.Web.Foundation.SwaggerSupport.Swashbuckle;
using Centurion.CloudManager.Web.Grpc;
using Centurion.CloudManager.Web.Services;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Composition;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation;
using Centurion.SeedWork.Web.Composition;
using Centurion.SeedWork.Web.Foundation;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using LightInject;
using MassTransit;

namespace Centurion.CloudManager;

public class Startup
{
  private const string ApiVersion = "v1";
  private const string ApiTitle = "Centurion Cloud Manager API";
  private readonly IConfiguration _configuration;
  private readonly IHostEnvironment _hostEnvironment;
  private IServiceContainer _container = null!;

  public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
  {
    _configuration = configuration;
    _hostEnvironment = hostEnvironment;
  }

  // This method gets called by the runtime. Use this method to add services to the container.
  // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
  public void ConfigureServices(IServiceCollection services)
  {
    services /*.AddHostedService<ServerInstancesStatsHostedService>()*/
      .AddHostedService<ImageRuntimeInfoWorker>()
      .AddHostedService<ScopedBackgroundWorkersExecutor>()
      .AddHostedService(s => (TokenBucketExecutionScheduler)s.GetRequiredService<IExecutionScheduler>());
    //
    // services.AddSingleton(c =>
    // {
    //   var config = c.GetRequiredService<AwsConfig>();
    //   return new AmazonEC2Client(config.AccessKeyId, config.SecretAccessKey,
    //     RegionEndpoint.GetBySystemName(config.PlacementRegion));
    // });
    // services.AddSingleton(c =>
    // {
    //   var config = c.GetService<GoogleComputeEngineConfig>();
    //   return new ComputeService(new BaseClientService.Initializer
    //   {
    //     ApplicationName = config.ApplicationName,
    //     HttpClientInitializer = GetCredential(config)
    //   });
    // });

    services.AddMassTransit(_ =>
      {
        _.AddConsumer<ComponentStatsConsumer>();
        _.SetSnakeCaseEndpointNameFormatter();
        _.UsingRabbitMq((ctx, cfg) =>
        {
          cfg.Host(_configuration.GetConnectionString("RabbitMq"));
          cfg.AddMessageDeserializer(new ContentType("application/json"),
            () => new ApplicationJsonMessageDeserializer(typeof(ComponentStatsEntry)));
          cfg.ConfigureEndpoints(ctx);
        });
      })
      .AddMassTransitHostedService();

    services
      .InitializeConfiguration(_configuration)
      .ConfigureCfgSectionAs<CheckoutServiceSpawnConfig>(_configuration, "CheckoutService")
      .ConfigureCfgSectionAs<LifetimeConfig>(_configuration, "Lifetime")
      .AddApplicationDbContext<CloudContext>(_hostEnvironment)
      .AddConfiguredCors(_configuration)
      .AddConfiguredMvc(c => c.AddApiExplorer())
      .AddConfiguredSignalR()
      .AddConfiguredAuthentication(_configuration)
      .AddConfiguredSwagger(ApiVersion, ApiTitle)
      .AddConfiguredTelemetry();


    services.AddIdentityServer(options => { options.EmitStaticAudienceClaim = true; })
      .AddDeveloperSigningCredential()
      .AddInMemoryClients(IdentityServerStaticConfig.GetClients())
      .AddInMemoryApiResources(IdentityServerStaticConfig.GetApiResources())
      .AddInMemoryIdentityResources(IdentityServerStaticConfig.GetIdentityResources())
      .AddTestUsers(IdentityServerStaticConfig.GetUsers());
    services.AddIdentityServerConfiguredCors(_configuration);
  }

  public void ConfigureContainer(IServiceContainer container)
  {
    container.RegisterFrom<ApplicationModule>();
    container.RegisterFrom<WebSeedWorkModule>();
    container.RegisterFrom<InfraSeedWorkModule>();

    _container = container;
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
    }

    var corsConfig = app.UseDefaultHttpBehavior(env);
    app.UseStaticFiles();
    app.UseIdentityServer();
    app.UseRouting();
    app.UseConfiguredCors(corsConfig);

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapGrpcService<CloudService>();
      endpoints.MapControllers();
      endpoints.MapDefaultControllerRoute();
    });

    app.UseConfiguredSwagger(ApiVersion, ApiTitle);
  }

  private static IConfigurableHttpClientInitializer GetCredential(GcpConfig config)
  {
    GoogleCredential credential = Task.Run(() => GoogleCredential.FromFile(config.CredentialsPath)).Result;
    if (credential.IsCreateScopedRequired)
    {
      credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    }

    return credential;
  }
}