using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Config;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Products.Controllers;

public class DashboardsController : SecuredDashboardBoundControllerBase
{
  private readonly DashboardsConfig _dashboardsConfig;
  private readonly IDashboardProvider _dashboardProvider;
  private readonly IDashboardService _dashboardService;
  private readonly IDashboardRepository _dashboardRepository;

  public DashboardsController(IServiceProvider provider, DashboardsConfig dashboardsConfig,
    IDashboardProvider dashboardProvider, IDashboardService dashboardService,
    IDashboardRepository dashboardRepository)
    : base(provider)
  {
    _dashboardsConfig = dashboardsConfig;
    _dashboardProvider = dashboardProvider;
    _dashboardService = dashboardService;
    _dashboardRepository = dashboardRepository;
  }

  [HttpGet("@product")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<ProductPublicInfoData?>))]
  public async ValueTask<IActionResult> GetCurrentAsync(CancellationToken ct)
  {
    var product = await _dashboardProvider.GetPublicByDashboardIdAsync(CurrentDashboardId, ct);
    if (product == null)
    {
      return NotFound();
    }

    return Ok(product);
  }

  [HttpGet("@my")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<DashboardData>))]
  public async ValueTask<IActionResult> GetOwnAsync(CancellationToken ct)
  {
    DashboardData? data = await _dashboardProvider.GetByOwnerIdAsync(CurrentUserId, ct);
    if (data == null)
    {
      return NotFound();
    }

    return Ok(data);
  }

  [HttpPut("@my")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> UpdateAsync([FromBody] DashboardData cmd, CancellationToken ct)
  {
    var dashboard = await _dashboardRepository.GetByOwnerIdAsync(CurrentUserId, ct);
    if (dashboard == null)
    {
      return NotFound();
    }

    await _dashboardService.UpdateAsync(dashboard, cmd, ct);
    return NoContent();
  }

  [HttpGet("Logins")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<DashboardLoginData>))]
  [AllowAnonymous]
  public async ValueTask<IActionResult> GetLoginData(string rawLocation, CancellationToken ct)
  {
    var modes = HostingConfig.ResolvePossibleModes(rawLocation, _dashboardsConfig);
    if (modes.IsFailure)
    {
      return BadRequest();
    }

    var data = await _dashboardProvider.GetLoginDataAsync(modes.Value, ct);
    if (data == null)
    {
      return NotFound();
    }

    return Ok(data);
  }
}