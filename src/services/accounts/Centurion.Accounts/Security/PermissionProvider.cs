using System.Reflection;
using Centurion.Accounts.App.Security.Model;
using Centurion.Accounts.App.Security.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Security;

public class PermissionProvider : IPermissionProvider
{
  private static readonly IDictionary<string, string> PermissionTranslations;

  private readonly IPermissionsRegistry _permissionsRegistry;

  static PermissionProvider()
  {
    PermissionTranslations = typeof(Permissions)
      .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
      .Where(_ => _.IsLiteral && !_.IsInitOnly)
      .ToDictionary(_ => (string) _.GetValue(null)!,
        _ => _.GetCustomAttribute<PermissionDescriptionAttribute>()?.Description ?? (string) _.GetValue(null)!);
  }
    
  public PermissionProvider(IPermissionsRegistry permissionsRegistry)
  {
    _permissionsRegistry = permissionsRegistry;
  }

  public IList<PermissionInfoData> GetSupportedPermissions()
  {
    return _permissionsRegistry.SupportedPermissions.Select(p => new PermissionInfoData
      {
        Permission = p,
        Description = PermissionTranslations.ContainsKey(p) ? PermissionTranslations[p] : p
      })
      .OrderBy(_ => _.Permission)
      .ToList();
  }
}