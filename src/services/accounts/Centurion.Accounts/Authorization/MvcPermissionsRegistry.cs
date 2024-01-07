using Centurion.Accounts.Foundation.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Centurion.Accounts.Core.Security.Services;

namespace Centurion.Accounts.Authorization;

public class MvcPermissionsRegistry : IPermissionsRegistry
{
  private readonly IDictionary<string, IReadOnlyList<string>> _permissionsCache;

  public MvcPermissionsRegistry(IActionDescriptorCollectionProvider provider)
  {
    _permissionsCache = provider.ActionDescriptors.Items
      .Select(_ => new
      {
        Controller = _.RouteValues["Controller"]?.ToString(),
        Action = _.RouteValues["Action"]?.ToString(),
        RequiresPermissions = _.EndpointMetadata.OfType<AuthorizePermissionAttribute>()
          .Select(r => r.Permission)
      })
      .Where(_ => _.RequiresPermissions.Any())
      .GroupBy(_ => ConstructKey(_.Controller!, _.Action!))
      .ToDictionary(_ => _.Key, _ => (IReadOnlyList<string>) _.SelectMany(r => r.RequiresPermissions)
        .ToList()
        .AsReadOnly());
  }

  public IEnumerable<string> SupportedPermissions => _permissionsCache.Values
    .SelectMany(_ => _)
    .Distinct();

  public IReadOnlyList<string> GetPermissions(params string[] keyTokens)
  {
    if (_permissionsCache.TryGetValue(ConstructKey(keyTokens), out var permissions))
    {
      return permissions;
    }

    return Array.Empty<string>();
  }

  private static string ConstructKey(params string[] keyTokens) => string.Join("___", keyTokens);
}