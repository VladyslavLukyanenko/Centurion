using Centurion.Accounts.App.Products.Collections;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Products.Controllers;

public class GlobalSearchController : SecuredDashboardBoundControllerBase
{
  private readonly IGlobalSearchProvider _globalSearchProvider;

  public GlobalSearchController(IServiceProvider provider, IGlobalSearchProvider globalSearchProvider)
    : base(provider)
  {
    _globalSearchProvider = globalSearchProvider;
  }

  [HttpGet]
  [AuthorizePermission(Permissions.ReleaseManage)]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<GlobalSearchPagedList>))]
  public async ValueTask<IActionResult> GetAsync([FromQuery] GlobalSearchPageRequest pageRequest,
    CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var page = await _globalSearchProvider.GetAllAsync(CurrentDashboardId, pageRequest, ct);
    return Ok(page);
  }
}