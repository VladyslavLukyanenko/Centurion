using Microsoft.AspNetCore.Authorization;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Foundation.Mvc.Controllers;

[Authorize(Policy = "SoftwareClientsOnly", AuthenticationSchemes = "LicenseKey")]
public abstract class SoftwareDashboardBoundControllerBase : SecuredControllerBase
{
  private string? _currentLicenseKey;

  protected SoftwareDashboardBoundControllerBase(IServiceProvider provider)
    : base(provider)
  {
  }

  protected string CurrentLicenseKey => _currentLicenseKey ??= User.GetLicenseKey()!;
}