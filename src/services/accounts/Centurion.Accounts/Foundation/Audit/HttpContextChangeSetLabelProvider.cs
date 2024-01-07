using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Centurion.Accounts.Core.Audit;
using Centurion.Accounts.Core.Audit.Services;

namespace Centurion.Accounts.Foundation.Audit;

public class HttpContextChangeSetLabelProvider : IChangeSetLabelProvider
{
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IDictionary<string, string> _scopes;

  public HttpContextChangeSetLabelProvider(IHttpContextAccessor httpContextAccessor,
    IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
  {
    _httpContextAccessor = httpContextAccessor;
    _scopes = actionDescriptorCollectionProvider.ActionDescriptors
      .Items
      .OfType<ControllerActionDescriptor>()
      .Where(_ => _.MethodInfo.GetCustomAttribute<AuditScopeAttribute>() != null)
      .ToDictionary(_ => GetScopeKey(_.ControllerName, _.ActionName),
        // ReSharper disable once PossibleNullReferenceException
        it => it.MethodInfo.GetCustomAttribute<AuditScopeAttribute>()!.ScopeName);
  }

  public string? GetLabel()
  {
    var http = _httpContextAccessor.HttpContext;
    var controller = http?.Request.RouteValues["controller"];
    var action = http?.Request.RouteValues["action"];
    if (controller == null || action == null)
    {
      return null;
    }

    var key = GetScopeKey(controller, action);
    return _scopes.TryGetValue(key, out var scope) ? scope : null;
  }

  private static string GetScopeKey(object controller, object action)
  {
    return (controller + "/" + action).ToLowerInvariant();
  }
}