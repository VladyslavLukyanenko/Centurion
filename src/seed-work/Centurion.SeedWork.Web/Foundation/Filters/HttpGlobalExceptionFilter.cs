using System.Text.Json;
using System.Text.Json.Serialization;
using Centurion.SeedWork.Web.Foundation.Authorization;
using Centurion.SeedWork.Web.Foundation.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Centurion.SeedWork.Web.Foundation.Filters;

public class HttpGlobalExceptionFilter : IExceptionFilter
{
  private readonly ILogger<HttpGlobalExceptionFilter> _logger;

  public HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger)
  {
    _logger = logger;
  }

  public void OnException(ExceptionContext context)
  {
    IActionResult result;
    int statusCode;
    if (context.Exception is AuthorizationException authorizationException)
    {
      _logger.LogWarning("Unauthorized: {ErrorMessage}", authorizationException.Message);
      var error = new ApiContract<object>(new ApiError(authorizationException));
      statusCode = StatusCodes.Status403Forbidden;
      result = CreateJsonResult(error);
    }
    else if (context.Exception is AppException appException)
    {
      _logger.LogWarning("Error: {ErrorMessage}", appException.Message);
      var error = new ApiContract<object>(new ApiError(appException));
      statusCode = StatusCodes.Status400BadRequest;
      result = CreateJsonResult(error);
    }
    else if (context.Exception is CoreException coreException)
    {
      var error = new ApiContract<object>(new ApiError(coreException));
      statusCode = StatusCodes.Status400BadRequest;
      result = CreateJsonResult(error);
    }
    else
    {
      var error = new ApiContract<object>(new ApiError("InternalServerError"));
      result = CreateJsonResult(error);
      statusCode = StatusCodes.Status500InternalServerError;
      _logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message);
    }

    context.Result = result;
    context.HttpContext.Response.StatusCode = statusCode;
  }

  private static JsonResult CreateJsonResult<T>(ApiContract<T> error)
  {
    return new(error)
    {
      SerializerSettings = new JsonSerializerOptions
      {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      }
    };
  }
}