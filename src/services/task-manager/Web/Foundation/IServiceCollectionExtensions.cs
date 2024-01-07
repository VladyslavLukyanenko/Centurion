using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation.Config;
using Centurion.SeedWork.Web.Foundation;
using Centurion.TaskManager.Infrastructure.Config;
using Centurion.TaskManager.Web.Foundation.Config;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Centurion.TaskManager.Web.Foundation;

// ReSharper disable once InconsistentNaming
public static class IServiceCollectionExtensions
{
  public static IServiceCollection InitializeConfiguration(this IServiceCollection services, IConfiguration cfg)
  {
    return services
      /*.ConfigureCfgSectionAs<CommonConfig>(cfg, CfgSectionNames.Common)
      .ConfigureCfgSectionAs<SsoConfig>(cfg, CfgSectionNames.Sso)*/
      .ConfigureCfgSectionAs<IntegrationBusConfig>(cfg, CfgSectionNames.Integration)
      .ConfigureCfgSectionAs<EfCoreConfig>(cfg, CfgSectionNames.EntityFramework);
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

  private static class CfgSectionNames
  {
    public const string Common = nameof(Common);
    public const string Idp = nameof(Idp);
    public const string Integration = nameof(Integration);
    public const string EntityFramework = nameof(EntityFramework);
  }
}