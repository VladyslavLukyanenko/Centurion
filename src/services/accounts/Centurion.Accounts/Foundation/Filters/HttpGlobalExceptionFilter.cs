using Centurion.Accounts.App;
using Centurion.Accounts.Core;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Centurion.Accounts.Foundation.Filters;

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
    return new JsonResult(error)
    {
      SerializerSettings = new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
      }
    };
  }
}