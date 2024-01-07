using System.Text.Json.Serialization;
using Centurion.SeedWork.Web.Foundation.ActionResults;
using Centurion.SeedWork.Web.Foundation.Filters;
using Centurion.SeedWork.Web.Foundation.FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Centurion.SeedWork.Web.Foundation;

// ReSharper disable once InconsistentNaming
public static class IServiceCollectionExtensions
{
  public static IServiceCollection ConfigureCfgSectionAs<T>(this IServiceCollection svc, IConfiguration cfg,
    string sectionName) where T : class
  {
    var section = cfg.GetSection(sectionName);
    svc.Configure<T>(section);
    T c = section.Get<T>();
    svc.AddSingleton(c);

    return svc;
  }

  public static IServiceCollection AddConfiguredMvc(this IServiceCollection services,
    Action<IMvcCoreBuilder>? mvcConfigurer = null)

  {
    var mvcBuilder = services
      .AddRouting(options =>
      {
        options.LowercaseUrls = true;
        // options.LowercaseQueryStrings = true;
      })
      .AddMvcCore(o =>
      {
        o.Filters.Add<HttpGlobalExceptionFilter>(int.MinValue);
        o.Filters.Add<TransactionScopeFilter>(int.MinValue + 1);
      })
      .AddControllersAsServices()
      .AddRazorPages(o => { o.RootDirectory = "/Web/Pages"; })
      .AddFluentValidation(_ =>
      {
        _.RegisterValidatorsFromAssembly(typeof(IServiceCollectionExtensions).Assembly);
        _.DisableDataAnnotationsValidation = true;
      })
      .AddJsonOptions(_ =>
      {
        _.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        _.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
      })
      .ConfigureApiBehaviorOptions(_ =>
      {
        _.InvalidModelStateResponseFactory = ctx =>
          new ValidationErrorResult(ctx.ModelState.Keys, ctx.HttpContext.Request.Path);
      })
      .AddViewLocalization(_ => _.ResourcesPath = "Resources");

    mvcConfigurer?.Invoke(mvcBuilder);

    services.AddGrpc(_ =>
    {
      _.MaxReceiveMessageSize = 1.Gb();
      _.MaxSendMessageSize = 1.Gb();
      // _.Interceptors.Add<GlobalExceptionInterceptor>();
      // _.Interceptors.Add<TransactionScopeInterceptor>();
    });
    services.AddMemoryCache()
      .AddResponseCompression();

    services.AddTransient<IValidatorInterceptor, ErrorCodesPopulatorValidatorInterceptor>();

    return services;
  }
}