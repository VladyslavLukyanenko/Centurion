using Centurion.Monitor.Composition;
using Centurion.Monitor.Web.Grpc;
using Centurion.SeedWork.Web.Foundation;
using LightInject;

namespace Centurion.Monitor;

public class Startup
{
  private readonly IConfiguration _configuration;
  private IServiceContainer _container = null!;

  public Startup(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services
      .AddRouting()
      .AddMvcCore();

    services.AddConfiguredTelemetry()
      .AddMemoryCache()
      .AddGrpc();
  }

  public void ConfigureContainer(IServiceContainer container)
  {
    container.RegisterFrom<ApplicationModule>();
    container.RegisterSingleton(_ => _);

    _container = container;
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    _container.Compile();
    app.UseConfiguredElasticApm(_configuration);

    if (env.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }

    app.UseCommonHttpBehavior(env);
    app.UseRouting();

    app.UseEndpoints(endpoints => { endpoints.MapGrpcService<MonitorService>(); });
  }
}