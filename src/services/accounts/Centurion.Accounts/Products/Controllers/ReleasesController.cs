using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Products.Controllers;

public class ReleasesController : SecuredDashboardBoundControllerBase
{
  private readonly IReleaseProvider _releaseProvider;
  private readonly IReleaseRepository _releaseRepository;
  private readonly IReleaseService _releaseService;

  public ReleasesController(IServiceProvider provider, IReleaseProvider releaseProvider,
    IReleaseRepository releaseRepository, IReleaseService releaseService)
    : base(provider)
  {
    _releaseProvider = releaseProvider;
    _releaseRepository = releaseRepository;
    _releaseService = releaseService;
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<IPagedList<ReleaseRowData>>))]
  [AuthorizePermission(Permissions.ReleaseManage)]
  public async ValueTask<IActionResult> GetPageAsync([FromQuery] ReleasesPageRequest pageRequest,
    CancellationToken ct)
  {
    var page = await _releaseProvider.GetPageAsync(CurrentDashboardId, pageRequest, ct);
    return Ok(page);
  }

  [HttpGet("Active")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<ActiveReleaseInfoData[]>))]
  public async ValueTask<IActionResult> GetActiveListAsync(CancellationToken ct)
  {
    var list = await _releaseProvider.GetActiveListAsync(CurrentDashboardId, ct);
    return Ok(list);
  }

  [HttpGet("{id:long}")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<ReleaseData>))]
  [AuthorizePermission(Permissions.ReleaseManage)]
  public async ValueTask<IActionResult> GetByIdAsync(long id, CancellationToken ct)
  {
    var data = await _releaseProvider.GetByIdAsync(CurrentDashboardId, id, ct);
    if (data == null)
    {
      return NotFound();
    }

    return Ok(data);
  }

  // +authorize dashboard member or admin
  [HttpGet("stock/{password}")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<ReleaseStockData>))]
  public async ValueTask<IActionResult> GetStockAsync(string password, CancellationToken ct)
  {
    var release = await _releaseProvider.GetStockByPasswordAsync(password, ct);
    if (release.HasNoValue)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrMemberAsync(release.Value!.DashboardId)
      .OrThrowForbid();

    return Ok(release.Value);
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.ReleaseManage)]
  public async ValueTask CreateAsync([FromBody] SaveReleaseCommand cmd, CancellationToken ct)
  {
    await _releaseService.CreateAsync(CurrentDashboardId, cmd, ct);
  }

  [HttpPut("{id:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.ReleaseManage)]
  public async ValueTask<IActionResult> UpdateAsync(long id, [FromBody] SaveReleaseCommand cmd, CancellationToken ct)
  {
    var release = await _releaseRepository.GetByIdAsync(id, ct);
    if (release == null)
    {
      return NotFound();
    }

    await _releaseService.UpdateAsync(release, cmd, ct);
    return NoContent();
  }

  [HttpPut("{id:long}/Active")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.ReleaseManage)]
  public async ValueTask<IActionResult> ToggleActiveAsync(long id, bool isActive, CancellationToken ct)
  {
    var release = await _releaseRepository.GetByIdAsync(id, ct);
    if (release == null)
    {
      return NotFound();
    }

    release.IsActive = isActive;
    _releaseRepository.Update(release);

    return NoContent();
  }

  [HttpDelete("{id:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.ReleaseDelete)]
  public async ValueTask<IActionResult> RemoveAsync(long id, CancellationToken ct)
  {
    Release? release = await _releaseRepository.GetByIdAsync(id, ct);
    if (release == null)
    {
      return NotFound();
    }

    _releaseRepository.Remove(release);
    return NoContent();
  }
}