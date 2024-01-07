using Centurion.Accounts.App.Config;
using Centurion.Accounts.Foundation.Audit;
using Centurion.Accounts.Foundation.Config;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Centurion.Accounts.Foundation;

public static class ApplicationBuilderExtensions
{
  public static IApplicationBuilder UseConfiguredSwagger(this IApplicationBuilder app, string apiVersion,
    string apiTitle)
  {
    app.UseSwagger(c => { });
    app.UseSwaggerUI(c => { c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", apiTitle); });
    return app;
  }

  public static CommonConfig UseCommonHttpBehavior(this IApplicationBuilder app, IWebHostEnvironment env)
  {
    var startupConfig = app.ApplicationServices.GetRequiredService<IOptions<StartupConfiguration>>().Value;

    if (!env.IsDevelopment())
    {
      app.UseHsts();
    }

    if (startupConfig.UseHttps)
    {
      app.UseHttpsRedirection();
    }

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
      ForwardedHeaders = ForwardedHeaders.All
    });

    return app.ApplicationServices.GetRequiredService<CommonConfig>();
  }

  public static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app,
    CommonConfig config)
  {
    if (config.Cors.UseCors)
    {
      app.UseCors("DefaultCors");
    }

    return app;
  }

  public static IApplicationBuilder UseConfiguredApm(this IApplicationBuilder app, IConfiguration configuration)
  {
    if (!configuration.GetSection("ElasticApm:Enabled").Get<bool>())
    {
      return app;
    }

    app.UseAllElasticApm(configuration);

    return app;
  }

  public static IApplicationBuilder UseAudit(this IApplicationBuilder app)
  {
    app.UseMiddleware<AuditMiddleware>();

    return app;
  }
}