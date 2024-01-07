using Centurion.SeedWork.Infra.EfCoreNpgsql;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

namespace Centurion.CloudManager.Web.Foundation.SwaggerSupport.Swashbuckle
{
  // ReSharper disable once InconsistentNaming
  public static class ISwashbuckleServiceCollectionExtensions
  {
    public static IServiceCollection AddConfiguredSwagger(this IServiceCollection services, string apiVersion,
      string apiTitle)
    {
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc(apiVersion, new OpenApiInfo { Title = apiTitle, Version = apiVersion });

        // var opsSet = new Dictionary<string, HashSet<string>>();
        c.CustomOperationIds(apiDesc =>
        {
          var api = (ControllerActionDescriptor)apiDesc.ActionDescriptor;
          var opName = (api.ControllerName + api.ActionName).ToCamelCase();
          // if (!opsSet.TryGetValue(api.ControllerName, out var actions))
          // {
          //     opsSet[api.ControllerName] = actions = new HashSet<string>();
          // }
          // if (actions.Contains(opName))
          // {
          //     throw new InvalidOperationException($"Action with name {api.ActionName} already exists in controller {api.ControllerName}");
          // }
          //
          // actions.Add(opName);
          return opName;
        });

        c.OperationFilter<FileUploadOperationFilter>();

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
          Scheme = "Bearer",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey,
          Description = "Please enter into field the word 'Bearer' following by space and JWT",
          Name = "Authorization"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme
            {
              Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            },
            Array.Empty<string>()
          }
        });

        c.DescribeAllParametersInCamelCase();
      });

      return services;
    }
  }
}