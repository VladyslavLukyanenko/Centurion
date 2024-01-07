using System.Security.Cryptography.X509Certificates;
using AspNetCoreRateLimit;
using Autofac;
using Centurion.Accounts.Announces.Hubs;
using Centurion.Accounts.App;
using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Foundation;
using Centurion.Accounts.Foundation.Config;
using Centurion.Accounts.Foundation.SwaggerSupport.Swashbuckle;
using Centurion.Accounts.Infra.Products.Consumers;
using Centurion.Accounts.Infra.Serialization.Json;
using Centurion.Accounts.Services;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Quartz;

namespace Centurion.Accounts;

public class Startup
{
  private const string ApiVersion = "v1";
  private const string ApiTitle = "Accounts API";
  private readonly IConfiguration _configuration;
  private readonly IWebHostEnvironment _environment;

  public Startup(IConfiguration configuration, IWebHostEnvironment environment)
  {
    _configuration = configuration;
    _environment = environment;
  }

  public void ConfigureContainer(ContainerBuilder builder)
  {
    builder.RegisterAssemblyModules(GetType().Assembly);
  }

  // This method gets called by the runtime. Use this method to add services to the container.
  // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
  public void ConfigureServices(IServiceCollection services)
  {
    var commonConf = _configuration.GetSection("Common").Get<CommonConfig>();
    var contractResolver =
      new ExtensibleCamelCasePropertyNamesContractResolver(new WwwrootPathsService(_environment, commonConf),
        _configuration);
    JsonConvert.DefaultSettings = NewtonsoftJsonSettingsFactory.CreateSettingsProvider(contractResolver);
    /*const string providerName = "GlobalInMemoryEfCoreCache";
    services.AddEFSecondLevelCache(o => o.UseEasyCachingCoreProvider(providerName)
        .DisableLogging(!_environment.IsDevelopment())
      /*.CacheAllQueries(CacheExpirationMode.Sliding, TimeSpan.FromSeconds(15))#1#);

    services.AddEasyCaching(o =>
    {
      o.UseInMemory(c => { c.DBConfig = new InMemoryCachingOptions(); }, providerName);
    });*/
    services
      .InitializeConfiguration(_configuration, _environment)
      .AddApplicationDbContext(_configuration)
      .AddConfiguredRateLimiter(_configuration)
      .AddConfiguredCors(_configuration)
      .AddConfiguredMvc(contractResolver)
      .AddConfiguredSignalR()
      .AddConfiguredAuthentication(_configuration)
      .AddConfiguredSwagger(ApiVersion, ApiTitle)
      .AddHttpClient(NamedHttpClients.DiscordClient);

    var s = _configuration.GetSection("IdentityServer");
    services.Configure<IdentityConfig>(s);
    var cfg = new IdentityConfig();
    s.Bind(cfg);

    services.AddConfiguredAspNetIdentity(_configuration);

    var signingCfg = _configuration.GetSection("Sso:Signing");
    var cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, signingCfg["Cert"]),
      signingCfg["Pwd"]);
    services.AddIdentityServer(options =>
      {
        var ssoCfg = _configuration.GetSection("Sso").Get<SsoConfig>();
        options.EmitStaticAudienceClaim = true;
        options.IssuerUri = ssoCfg.ValidIssuer;
      })
      .AddExtensionGrantValidator<DiscordIdTokenGrantValidator>()
      .AddExtensionGrantValidator<DiscordRefreshTokenGrantValidator>()
      .AddExtensionGrantValidator<LicenseKeyGrantValidator>()
      .AddAspNetIdentity<User>()
      // .AddDeveloperSigningCredential()
      .AddSigningCredential(cert)
      .AddInMemoryClients(IdentityServerStaticConfig.GetClients(cfg))
      .AddInMemoryApiResources(IdentityServerStaticConfig.GetApiResources(cfg))
      .AddInMemoryIdentityResources(IdentityServerStaticConfig.GetIdentityResources())
      .AddProfileService<ProfileService>();

    services.AddAuthorization(configure =>
    {
      configure.AddPolicy("SoftwareClientsOnly", b =>
        b.RequireScope("dashboards-software")
          .AddAuthenticationSchemes("LicenseKey")
          .RequireAuthenticatedUser());
    });

    services.AddScoped<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>()
      .AddScoped<IProfileService, ProfileService>();
    services.AddIdentityServerConfiguredCors(_configuration);

    // RateLimiting
    services.AddHttpContextAccessor();
    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    services.AddMassTransit(_ =>
      {
        _.AddConsumer<LicenseKeyAssociationChangeConsumer>();
        _.SetKebabCaseEndpointNameFormatter();
        _.UsingRabbitMq((ctx, c) =>
        {
          var connStrs = ctx.GetRequiredService<ConnectionStrings>();

          c.Host(connStrs.RabbitMq);
          // var schedulerConfig = ctx.GetService<SchedulerConfig>();
          c.ConfigureEndpoints(ctx);
          // c.UseInMemoryScheduler(); //schedulerConfig.QueueName);
        });
      })
      .AddMassTransitHostedService();

    services.Configure<QuartzOptions>(_configuration.GetSection("Quartz"));
    services.AddQuartz(_ => { _.UseMicrosoftDependencyInjectionJobFactory(); });

    services.AddQuartzServer(_ => { _.WaitForJobsToComplete = true; });
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    // RateLimiting
    app.UseClientRateLimiting();
    app.UseResponseCompression();

    app.UseConfiguredApm(_configuration);
    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }

    var corsConfig = app.UseCommonHttpBehavior(env);
    if (_configuration.GetValue<bool>("Sso:ForceHttpsHack"))
    {
      app.Use((context, next) =>
      {
        context.Request.Scheme = "https";
        return next();
      });
    }
    app.UseStaticFiles(new StaticFileOptions
    {
      FileProvider = new CompositeFileProvider(
        app.ApplicationServices.GetRequiredService<IArtifactsFileProvider>(),
        env.WebRootFileProvider
      )
    });
    app.UseRouting();
    app.UseConfiguredCors(corsConfig);

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseIdentityServer();

    app.UseAudit();
    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllers();
      endpoints.MapDefaultControllerRoute();
      endpoints.MapHub<AnnouncesHub>("/hubs/announces");
    });

    app.UseConfiguredSwagger(ApiVersion, ApiTitle);
  }
}