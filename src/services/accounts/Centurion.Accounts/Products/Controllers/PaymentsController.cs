using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Centurion.Accounts.Products.Commands;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using Stripe;

namespace Centurion.Accounts.Products.Controllers;

public class PaymentsController : SecuredDashboardBoundControllerBase
{
  private readonly IStripeGateway _stripeGateway;
  private readonly ILicenseKeyPaymentsService _licenseKeyPaymentsService;
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IReleaseRepository _releaseRepository;
  private readonly IUserRepository _userRepository;

  public PaymentsController(IServiceProvider provider, IStripeGateway stripeGateway,
    ILicenseKeyPaymentsService licenseKeyPaymentsService, IReleaseRepository releaseRepository,
    IDashboardRepository dashboardRepository, IUserRepository userRepository)
    : base(provider)
  {
    _stripeGateway = stripeGateway;
    _licenseKeyPaymentsService = licenseKeyPaymentsService;
    _releaseRepository = releaseRepository;
    _dashboardRepository = dashboardRepository;
    _userRepository = userRepository;
  }

  [HttpGet("Panel")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<string>))]
  public async ValueTask<IActionResult> GetBillingPortalLinkAsync(CancellationToken ct)
  {
    var user = await _userRepository.GetByIdAsync(CurrentUserId, ct);
    if (string.IsNullOrEmpty(user!.StripeCustomerId))
    {
      return NotFound();
    }

    var dashboard = await _dashboardRepository.GetByIdAsync(CurrentDashboardId, ct);
    try
    {
      var result = await _stripeGateway.OpenBillingPortalSessionAsync(user.StripeCustomerId, dashboard!, ct);
      if (result.IsFailure)
      {
        return BadRequest();
      }

      return Ok(result.Value);
    }
    catch (StripeException exc)
    {
      return BadRequest(exc.StripeError);
    }
  }

  // +authorize dashboard member or admin
  [HttpPost]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<string>))]
  public async ValueTask<IActionResult> CreatePaymentAsync([FromBody] CreatePaymentCommand cmd, CancellationToken ct)
  {
    string? stripeCustomerId = User.GetStripeCustomerId();

    Release? drop = await _releaseRepository.GetValidByPasswordAsync(cmd.Password, ct);
    if (drop == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrMemberAsync(drop.DashboardId)
      .OrThrowForbid();

    var dashboard = await _dashboardRepository.GetByIdAsync(CurrentDashboardId, ct);
    var r = await _stripeGateway.CreatePaymentSessionAsync(drop, dashboard!, stripeCustomerId, ct);
    return r.IsFailure
      ? BadRequest()
      : Ok(r.Value);
  }

  [HttpPost("{sessionId}/Process")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> ProcessPaymentAsync(string sessionId, CancellationToken ct)
  {
    var result = await _stripeGateway.GetSessionDataAsync(CurrentDashboardId, sessionId, ct);
    if (result.IsFailure)
    {
      return BadRequest();
    }

    var data = result.Value;
    var decrementResult = await GetDecrementedReleaseAsync(data.Password, ct);
    if (decrementResult.IsFailure)
    {
      return BadRequest();
    }

    Result<bool> isCapturedResult = await _stripeGateway.IsCapturedAsync(CurrentDashboardId, data.Intent, ct);
    if (isCapturedResult.IsFailure || !isCapturedResult.Value)
    {
      return BadRequest();
    }

    User? user = await _userRepository.GetByIdAsync(CurrentUserId, ct);
    if (user == null)
    {
      return BadRequest();
    }

    var discordToken = User.GetDiscordAccessToken();
    var dashboard = await _dashboardRepository.GetByIdAsync(CurrentDashboardId, ct);
    var processingResult = await _licenseKeyPaymentsService.ProcessPaymentAsync(dashboard!, data.PlanId,
      decrementResult.Value, data.Intent, data.Customer, user, discordToken!, ct);

    return processingResult.IsFailure
      ? BadRequest()
      : NoContent();
  }


  [HttpDelete("{sessionId}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask CancelPaymentAsync(string sessionId, CancellationToken ct)
  {
    await _stripeGateway.CancelSubscriptionAsync(CurrentDashboardId, sessionId, ct);
  }

  // authorize dashboard member or admin
  [HttpPost("{password}/Trial")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> ObtainTrialAsync(string password, CancellationToken ct)
  {
    var decrementResult = await GetDecrementedReleaseAsync(password, ct);
    if (decrementResult.IsFailure)
    {
      return BadRequest();
    }

    User? user = await _userRepository.GetByIdAsync(CurrentUserId, ct);
    if (user == null)
    {
      return BadRequest();
    }

    var discordToken = User.GetDiscordAccessToken();
    var dashboard = await _dashboardRepository.GetByIdAsync(CurrentDashboardId, ct);
    var obtainResult = await _licenseKeyPaymentsService.AcquireTrialKeyAsync(dashboard!, decrementResult.Value.PlanId,
      decrementResult.Value, user, discordToken!, ct);

    return obtainResult.IsFailure
      ? BadRequest()
      : NoContent();
  }

  private async ValueTask<Result<Release>> GetDecrementedReleaseAsync(string password, CancellationToken ct)
  {
    Release? release = await _releaseRepository.GetValidByPasswordAsync(password, ct);
    if (release == null)
    {
      return Result.Failure<Release>("Can't find drop");
    }

    await AppAuthorizationService.AdminOrMemberAsync(release.DashboardId)
      .OrThrowForbid();

    Result decrementResult = await _releaseRepository.DecrementAsync(release, ct);
    if (decrementResult.IsFailure)
    {
      return Result.Failure<Release>("No available item to buy");
    }

    return release;
  }
}