using Centurion.Accounts.App.Security.Model;
using Centurion.Accounts.App.Security.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Security.Controllers;

public class PermissionsController : SecuredDashboardBoundControllerBase
{
  private readonly IPermissionProvider _permissions;

  public PermissionsController(IServiceProvider provider, IPermissionProvider permissions)
    : base(provider)
  {
    _permissions = permissions;
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<PermissionInfoData[]>))]
  [AuthorizePermission(Permissions.RolesManage)]
  public async ValueTask<IActionResult> GetUsedPermissions()
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    return Ok(_permissions.GetSupportedPermissions());
  }
}