using System.Security.Claims;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Centurion.Accounts.Services;

public class LicenseKeyGrantValidator : IExtensionGrantValidator
{
  public const string GrantTypeName = "license-key";
  private readonly ILicenseKeyRepository _licenseKeyRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserProfileService _userProfileService;
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IUserRepository _userRepository;
  private readonly IClaimsProvider _claimsProvider;

  public LicenseKeyGrantValidator(ILicenseKeyRepository licenseKeyRepository, IUnitOfWork unitOfWork,
    IUserProfileService userProfileService, IDashboardRepository dashboardRepository, IUserRepository userRepository,
    IClaimsProvider claimsProvider)
  {
    _licenseKeyRepository = licenseKeyRepository;
    _unitOfWork = unitOfWork;
    _userProfileService = userProfileService;
    _dashboardRepository = dashboardRepository;
    _userRepository = userRepository;
    _claimsProvider = claimsProvider;
  }

  public async Task ValidateAsync(ExtensionGrantValidationContext context)
  {
    var keyValue = context.Request.Raw["key"];
    var hwid = context.Request.Raw["hwid"];
    var dashboardIdRaw = context.Request.Raw["did"];
    if (string.IsNullOrEmpty(keyValue) || string.IsNullOrEmpty(hwid) || string.IsNullOrEmpty(dashboardIdRaw)
        || !Guid.TryParse(dashboardIdRaw, out var dashboardId))
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
      return;
    }

    LicenseKey? key = await _licenseKeyRepository.GetByValueAsync(keyValue);
    if (key == null)
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Key not found");
      return;
    }

    if (key.IsExpired())
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Key is expired");
      return;
    }

    if (key.IsSuspended())
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Key is suspended");
      return;
    }

    if (key.IsUnbound())
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Key is unbound");
      return;
    }

    if (dashboardId != key.DashboardId)
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Invalid key provided");
      return;
    }

    if (!key.IsSessionValid(hwid))
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Machine mismatch");
      return;
    }

    key.BindSessionIfEmpty(hwid);
    key.RefreshActivity();
    _licenseKeyRepository.Update(key);
    var userId = key.UserId!.Value;
    await _userProfileService.RefreshUserProfileIfOutdatedAsync(userId, key.DashboardId);
    await _unitOfWork.SaveEntitiesAsync();

    var user = await _userRepository.GetByIdAsync(userId);
    var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId);
    var claims = new List<Claim>
    {
      new(AppClaimNames.LicenseKey, key.Value),
    };

    var absoluteExpiry = key.CalculateAbsoluteExpiry();
    if (absoluteExpiry.HasValue)
    {
      claims.Add(new Claim(AppClaimNames.LicenseKeyExpiry, absoluteExpiry.Value.ToString()));
    }

    claims.AddRange(_claimsProvider.GetDashboardClaims(dashboard!));
    claims.AddRange(_claimsProvider.GetUserClaims(user!));
    claims.AddRange(await _claimsProvider.GetPermissionClaimsAsync(user!, dashboard!));

    context.Result = new GrantValidationResult(key.UserId.ToString(), GrantType, claims);
  }

  public string GrantType => GrantTypeName;
}