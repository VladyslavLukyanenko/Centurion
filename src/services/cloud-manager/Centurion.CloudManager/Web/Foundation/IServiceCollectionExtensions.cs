using Centurion.CloudManager.App;
using Centurion.CloudManager.Infra.Services.Aws;
using Centurion.CloudManager.Infra.Services.Gcp;
using Centurion.CloudManager.Web.Foundation.Config;
using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation.Config;
using Centurion.SeedWork.Web.Foundation;
using IdentityModel;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Centurion.CloudManager.Web.Foundation
{
  // ReSharper disable once InconsistentNaming
  public static class IServiceCollectionExtensions
  {
    private static class CfgSectionNames
    {
      public const string Aws = nameof(Aws);
      public const string Gcp = nameof(Gcp);
      public const string Cors = nameof(Cors);
      public const string EntityFramework = nameof(EntityFramework);
      public const string DockerAuth = "DockerAuth:DockerHub";
    }

    public static IServiceCollection InitializeConfiguration(this IServiceCollection services, IConfiguration cfg)
    {
      return services.Configure<JsonSerializerSettings>(ConfigureJsonSerializerSettings)
        .ConfigureCfgSectionAs<CorsConfig>(cfg, CfgSectionNames.Cors)
        .ConfigureCfgSectionAs<AwsConfig>(cfg, CfgSectionNames.Aws)
        .ConfigureCfgSectionAs<GcpConfig>(cfg, CfgSectionNames.Gcp)
        .ConfigureCfgSectionAs<LoginPwdDockerAuthConfig>(cfg, CfgSectionNames.DockerAuth)
        .ConfigureCfgSectionAs<EfCoreConfig>(cfg, CfgSectionNames.EntityFramework);
    }

    public static IServiceCollection AddConfiguredCors(this IServiceCollection services,
      IConfiguration cfg)
    {
      return services.AddCors(options =>
      {
        options.AddPolicy("DefaultCors", policy =>
        {
          var conf = cfg.GetSection(CfgSectionNames.Cors).Get<CorsConfig>();
          policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()

            // NOTICE: not allowed "any (*)" origin with credentials
            .WithOrigins(conf.AllowedHosts.ToArray());
        });
      });
    }

    public static IServiceCollection AddIdentityServerConfiguredCors(this IServiceCollection services,
      IConfiguration cfg)
    {
      services.AddSingleton<ICorsPolicyService>(s =>
      {
        var loggerFactory = s.GetRequiredService<ILoggerFactory>();
        var conf = cfg.GetSection(CfgSectionNames.Cors).Get<CorsConfig>();
        var cors = new DefaultCorsPolicyService(loggerFactory.CreateLogger<DefaultCorsPolicyService>());
        foreach (var host in conf.AllowedHosts)
        {
          cors.AllowedOrigins.Add(host);
        }

        return cors;
      });

      return services;
    }

    public static IServiceCollection AddConfiguredSignalR(this IServiceCollection services)
    {
      services.AddSignalR(_ =>
        {
          // configure here...
        })
        .AddNewtonsoftJsonProtocol();

      return services;
    }

    public static IServiceCollection AddConfiguredAuthentication(this IServiceCollection services, IConfiguration cfg)
    {
      var config = cfg.GetSection("IdentityServer").Get<IdentityServerConfig>();
      services.Configure<TokenValidationParameters>(o => ToTokenValidationParameters(o, config));
      services.AddAuthentication(options =>
        {
          options.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
          options.DefaultChallengeScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
          options.DefaultForbidScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
          options.DefaultSignInScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
          options.DefaultSignOutScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
          options.DefaultScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
        })
        .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, options =>
          {
            options.Audience = config.ValidAudience;
            options.Authority = config.AuthorityUrl;
            options.RequireHttpsMetadata = config.RequireHttpsMetadata;
            options.TokenValidationParameters = ToTokenValidationParameters(new TokenValidationParameters(), config);
            options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
            options.Events = new JwtBearerEvents
            {
              OnMessageReceived = context =>
              {
                var accessToken = context.Request.Query["access_token"];
                var path = context.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                  context.Token = accessToken;
                }

                return Task.CompletedTask;
              }
            };
          },
          options =>
          {
            options.NameClaimType = JwtClaimTypes.Id;
            options.ClientId = IdentityServerStaticConfig.ClientId;
            options.ClientSecret = IdentityServerStaticConfig.ClientSecret;
            options.Authority = config.AuthorityUrl;
            options.Validate();
          });

      return services;
    }

    private static TokenValidationParameters ToTokenValidationParameters(TokenValidationParameters target,
      IdentityServerConfig cfg)
    {
      target.ValidateAudience = cfg.ValidateAudience;
      target.ValidateIssuer = cfg.ValidateIssuer;
      target.ValidateLifetime = cfg.ValidateLifetime;
      target.RequireExpirationTime = true;

      target.ValidIssuer = cfg.ValidIssuer;
      target.ValidAudience = cfg.ValidAudience;

      // target.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.IssuerSigningKey));

      return target;
    }


    private static void ConfigureJsonSerializerSettings(JsonSerializerSettings serializerSettings)
    {
      serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
      serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
      serializerSettings.NullValueHandling = NullValueHandling.Ignore;
      serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
      serializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
      serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

      JsonConvert.DefaultSettings = () => serializerSettings;
    }
  }
}