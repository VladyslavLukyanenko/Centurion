using Centurion.Accounts.App.Security.Model;

namespace Centurion.Accounts.App.Security.Services;

public interface IPermissionProvider
{
  IList<PermissionInfoData> GetSupportedPermissions();
}