using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Core.Collections;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Centurion.Accounts.Products.Controllers;

public class LicenseKeysController : SecuredDashboardBoundControllerBase
{
  private readonly ILicenseKeyRepository _licenseKeyRepository;
  private readonly IUserRepository _userRepository;
  private readonly ILicenseKeyService _licenseKeyService;
  private readonly ILicenseKeyProvider _licenseKeyProvider;
  private readonly IPlanRepository _planRepository;

  public LicenseKeysController(IServiceProvider provider, ILicenseKeyService licenseKeyService,
    ILicenseKeyRepository licenseKeyRepository, IUserRepository userRepository,
    ILicenseKeyProvider licenseKeyProvider, IPlanRepository planRepository)
    : base(provider)
  {
    _licenseKeyService = licenseKeyService;
    _licenseKeyRepository = licenseKeyRepository;
    _userRepository = userRepository;
    _licenseKeyProvider = licenseKeyProvider;
    _planRepository = planRepository;
  }

  [HttpGet("@my")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<PurchasedLicenseKeyData[]>))]
  public async ValueTask<IActionResult> GetPurchasedLicenseKeys(CancellationToken ct)
  {
    var licenseKeys = await _licenseKeyProvider.GetPurchasedKeysAsync(CurrentDashboardId, CurrentUserId, ct);
    return Ok(licenseKeys);
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<IPagedList<LicenseKeySnapshotData>>))]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> GetLicenseKeys([FromQuery] LicenseKeyPageRequest pageRequest,
    CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var licenseKeys = await _licenseKeyProvider.GetPageAsync(CurrentDashboardId, pageRequest, ct);
    return Ok(licenseKeys);
  }

  [HttpGet("{id:long}/Summary")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<LicenseKeySummaryData>))]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> GetLicenseKeySummaryAsync(long id, CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var summary = await _licenseKeyProvider.GetSummaryByIdAsync(CurrentDashboardId, id, ct);
    return Ok(summary);
  }

  [HttpPatch("{id:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> UpdateLicenseKeyAsync(long id, [FromBody] UpdateLicenseKeyCommand cmd,
    CancellationToken ct)
  {
    LicenseKey? licenseKey = await _licenseKeyRepository.GetByIdAsync(id, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId)
      .OrThrowForbid();

    var result = await _licenseKeyService.UpdateAsync(licenseKey, cmd, ct);
    if (result.IsFailure)
    {
      return BadRequest(result.Error.ToApiError());
    }

    return NoContent();
  }

  [HttpPatch("{id:long}/Prolong")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> ProlongLicenseKeyAsync(long id, int daysToAdd, CancellationToken ct)
  {
    LicenseKey? licenseKey = await _licenseKeyRepository.GetByIdAsync(id, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId)
      .OrThrowForbid();


    var result = licenseKey.Prolong(Duration.FromDays(daysToAdd));
    if (result.IsFailure)
    {
      return BadRequest(result.Error.ToApiError());
    }

    _licenseKeyRepository.Update(licenseKey);

    return NoContent();
  }

  [HttpGet("Releases/{releaseId:long}")]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  [AuthorizePermission(Permissions.ReleaseManage)]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<IPagedList<LicenseKeyShortData>>))]
  public async ValueTask<IActionResult> GetPageByReleaseIdAsync(long releaseId, [FromQuery] PageRequest pageRequest,
    CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var licenseKeys =
      await _licenseKeyProvider.GetPageByReleaseIdAsync(CurrentDashboardId, releaseId, pageRequest, ct);
    return Ok(licenseKeys);
  }

  // +authorize dashboard owner or admin
  [HttpGet("UsedToday")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<int>))]
  [AuthorizePermission(Permissions.LicenseKeysStatsUsedCount)]
  public async ValueTask<IActionResult> GetUsedTodayCountAsync(Offset offset, CancellationToken ct)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var count = await _licenseKeyProvider.GetUsedTodayCountAsync(CurrentDashboardId, offset, ct);
    return Ok(count);
  }

  // +authorize dashboard owner or admin
  [HttpPatch("{key}/Suspend")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.LicenseKeysToggleSuspend)]
  public async ValueTask<IActionResult> SuspendKeyAsync(string key, CancellationToken ct)
  {
    var licenseKey = await _licenseKeyRepository.GetByValueAsync(key, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId)
      .OrThrowForbid();

    var suspendResult = await _licenseKeyService.SuspendAsync(licenseKey, ct);
    if (suspendResult.IsFailure)
    {
      return BadRequest();
    }

    return NoContent();
  }

  [HttpPost("Generate")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> GenerateAsync([FromBody] GenerateLicenseKeysCommand cmd, CancellationToken ct)
  {
    Plan? plan = await _planRepository.GetByIdAsync(cmd.PlanId, ct);
    if (plan == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(plan.DashboardId)
      .OrThrowForbid();

    await _licenseKeyService.GenerateKeys(plan, cmd, ct);
    return NoContent();
  }

  // +authorize dashboard owner or admin
  [HttpPatch("Plans/{planId:long}/Suspend")]
  [AuthorizePermission(Permissions.LicenseKeysToggleSuspend)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> SuspendByPlanIdAsync(long planId, CancellationToken ct = default)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var result = await _licenseKeyService.SuspendAllAsync(planId, ct);
    if (result.IsFailure)
    {
      return BadRequest();
    }

    return NoContent();
  }

  // +authorize dashboard owner or admin
  [HttpPatch("Plans/{planId:long}/Resume")]
  [AuthorizePermission(Permissions.LicenseKeysToggleSuspend)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> ResumeByPlanIdAsync(long planId, CancellationToken ct = default)
  {
    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(CurrentDashboardId)
      .OrThrowForbid();

    var result = await _licenseKeyService.ResumeAllAsync(planId, ct);
    if (result.IsFailure)
    {
      return BadRequest();
    }

    return NoContent();
  }

  // +authorize dashboard owner or admin
  [HttpPatch("{key}/Resume")]
  [AuthorizePermission(Permissions.LicenseKeysToggleSuspend)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> ResumeKeyAsync(string key, CancellationToken ct)
  {
    var licenseKey = await _licenseKeyRepository.GetByValueAsync(key, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId)
      .OrThrowForbid();

    var suspendResult = await _licenseKeyService.ResumeAsync(licenseKey, ct);
    if (suspendResult.IsFailure)
    {
      return BadRequest();
    }

    return NoContent();
  }

  // +authorize dashboard owner or admin
  [HttpPatch("{key}/Reset")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> ResetAsync(string key, CancellationToken ct)
  {
    var licenseKey = await _licenseKeyRepository.GetByValueAsync(key, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrSameUserAsync(licenseKey.UserId.GetValueOrDefault())
      .Or(async () => await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId))
      .OrThrowForbid();

    licenseKey.ResetSession();
    _licenseKeyRepository.Update(licenseKey);

    return NoContent();
  }

  // +authorize dashboard member or admin
  [HttpPost("{key}/Bind")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<bool>))]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> BindToUserAsync(string key, long? userId, CancellationToken ct = default)
  {
    var targetUserId = userId.GetValueOrDefault(CurrentUserId);
    User? user = await _userRepository.GetByIdAsync(targetUserId, ct);
    LicenseKey? licenseKey = await _licenseKeyRepository.GetByValueAsync(key, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrSameUserAsync(targetUserId)
      .Or(async () => await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId))
      .OrThrowForbid();

    string? discordToken = null;
    if (targetUserId != CurrentUserId)
    {
      discordToken = User.GetDiscordAccessToken();
      if (string.IsNullOrEmpty(discordToken))
      {
        return BadRequest();
      }
    }

    var result = await _licenseKeyService.BindAsync(licenseKey, user!, discordToken, ct);
    if (result.IsFailure)
    {
      return BadRequest();
    }

    return Ok(licenseKey.IsLifetime());
  }

  // +authorize key owner or dashboard owner or admin
  [HttpDelete("{key}/Unbind")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<string>))]
  [AuthorizePermission(Permissions.LicenseKeysManage)]
  public async ValueTask<IActionResult> UnbindFromUserAsync(string key, CancellationToken ct)
  {
    LicenseKey? licenseKey = await _licenseKeyRepository.GetByValueAsync(key, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrSameUserAsync(licenseKey.UserId.GetValueOrDefault())
      .Or(async () => await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId))
      .OrThrowForbid();

    var unboundResult = await _licenseKeyService.UnbindAsync(licenseKey, ct);
    if (unboundResult.IsFailure)
    {
      return BadRequest();
    }

    return Ok(licenseKey.Value);
  }

  // +authorize key owner or dashboard owner or admin
  [HttpDelete("{id:long}")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<bool>))]
  [AuthorizePermission(Permissions.LicenseKeysDelete)]
  public async ValueTask<IActionResult> RemoveAsync(long id, CancellationToken ct)
  {
    LicenseKey? licenseKey = await _licenseKeyRepository.GetByIdAsync(id, ct);
    if (licenseKey == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrSameUserAsync(licenseKey.UserId.GetValueOrDefault())
      .Or(async () => await AppAuthorizationService.AuthorizeCurrentPermissionsAsync(licenseKey.DashboardId))
      .OrThrowForbid();

    var unboundResult = await _licenseKeyService.RemoveKeyAsync(licenseKey, ct);
    if (unboundResult.IsFailure)
    {
      return BadRequest();
    }

    return Ok(licenseKey.Value);
  }
}