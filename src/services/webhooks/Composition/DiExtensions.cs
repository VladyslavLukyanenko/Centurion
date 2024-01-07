using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation.Config;
using Centurion.TaskManager.Infrastructure.Config;
using Centurion.TaskManager.Web.Foundation.Config;
using Elastic.Apm.NetCoreAll;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

namespace Centurion.WebhookSender.Composition;

public static class DiExtensions
{
  private static class CfgSectionNames
  {
    public const string Idp = nameof(Idp);
    public const string Integration = nameof(Integration);
    public const string EntityFramework = nameof(EntityFramework);
  }

  public static IServiceCollection InitializeConfiguration(this IServiceCollection services, IConfiguration cfg)
  {
    return services
      .ConfigureCfgSectionAs<IntegrationBusConfig>(cfg, CfgSectionNames.Integration)
      .ConfigureCfgSectionAs<EfCoreConfig>(cfg, CfgSectionNames.EntityFramework);
  }

  private static IServiceCollection ConfigureCfgSectionAs<T>(this IServiceCollection svc, IConfiguration cfg,
    string sectionName) where T : class
  {
    var section = cfg.GetSection(sectionName);
    svc.Configure<T>(section);
    T c = section.Get<T>();
    svc.AddSingleton(c);

    return svc;
  }

  public static IServiceCollection AddConfiguredAuthentication(this IServiceCollection services, IConfiguration cfg)
  {
    services.AddAuthentication();
    var idpConfig = cfg.GetSection(CfgSectionNames.Idp).Get<IdpConfig>();
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, jwt =>
        {
          jwt.Authority = idpConfig.AuthorityUrl;
          jwt.RequireHttpsMetadata = idpConfig.RequireHttpsMetadata;
          jwt.TokenValidationParameters = ToTokenValidationParameters(jwt.TokenValidationParameters, idpConfig);
        },
        introspect =>
        {
          introspect.NameClaimType = JwtClaimTypes.Id;
          introspect.RoleClaimType = JwtClaimTypes.Role;

          introspect.Authority = idpConfig.AuthorityUrl;
          introspect.ClientId = idpConfig.ClientId;
          introspect.ClientSecret = idpConfig.ClientSecret;
          introspect.Validate();
        });

    return services;
  }

  public static IServiceCollection AddConfiguredAuthorization(this IServiceCollection services, IConfiguration cfg)
  {
    services.AddAuthorization();

    return services;
  }


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
    app.UseAllElasticApm(configuration);

    return app;
  }

  private static TokenValidationParameters ToTokenValidationParameters(TokenValidationParameters? target,
    IdpConfig cfg)
  {
    target ??= new TokenValidationParameters();
    target.ValidateAudience = cfg.ValidateAudience;
    target.ValidateIssuer = cfg.ValidateIssuer;
    target.ValidateLifetime = cfg.ValidateLifetime;
    target.RequireExpirationTime = true;

    target.ValidIssuer = cfg.ValidIssuer;
    target.ValidAudience = cfg.ValidAudience;
    target.NameClaimType = JwtClaimTypes.Id;
    target.RoleClaimType = JwtClaimTypes.Role;

    return target;
  }
}