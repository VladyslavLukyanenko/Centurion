using Centurion.CloudManager.Web.Foundation.Config;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Centurion.CloudManager.Web.Foundation
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseConfiguredSwagger(this IApplicationBuilder app, string apiVersion,
      string apiTitle)
    {
      app.UseSwagger(c => { });
      app.UseSwaggerUI(c => { c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", apiTitle); });
      return app;
    }

    public static CorsConfig UseDefaultHttpBehavior(this IApplicationBuilder app, IWebHostEnvironment env)
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

      return app.ApplicationServices.GetRequiredService<CorsConfig>();
    }

    public static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app,
      CorsConfig startupConfig)
    {
      if (startupConfig.UseCors)
      {
        app.UseCors("DefaultCors");
      }

      return app;
    }
  }
}