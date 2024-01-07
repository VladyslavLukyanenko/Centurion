using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Centurion.SeedWork.Web.Foundation;

public static class ApplicationBuilderExtensions
{
  public static void UseCommonHttpBehavior(this IApplicationBuilder app, IWebHostEnvironment env)
  {
    if (!env.IsDevelopment())
    {
      app.UseHsts();
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
      ForwardedHeaders = ForwardedHeaders.All
    });
  }

  public static IApplicationBuilder UseConfiguredElasticApm(this IApplicationBuilder app,
    IConfiguration configuration)
  {
    if (!configuration.GetSection("ElasticApm:Enabled").Get<bool>())
    {
      return app;
    }

    // app.UseElasticApm(configuration,
    // 	new HttpDiagnosticsSubscriber(),
    // 	new EfCoreDiagnosticsSubscriber(),
    // 	new SqlClientDiagnosticSubscriber(),
    // 	new ElasticsearchDiagnosticsSubscriber(),
    // 	new GrpcClientDiagnosticSubscriber()
    // );

    app.UseAllElasticApm(configuration);

    // var redisConn = app.ApplicationServices.GetRequiredService<IRedisCacheConnectionPoolManager>();
    // redisConn.GetConnection().UseElasticApm();

    // Agent.Subscribe(new MassTransitDiagnosticsSubscriber());

    return app;
  }
}