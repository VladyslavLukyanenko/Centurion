using Centurion.Accounts.Authorization;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Foundation.Mvc.Controllers;

public abstract class SecuredDashboardBoundControllerBase : SecuredControllerBase
{
  private Guid? _currentDashboardId;
  protected readonly IAppAuthorizationService AppAuthorizationService;

  protected SecuredDashboardBoundControllerBase(IServiceProvider provider)
    : base(provider)
  {
    AppAuthorizationService = provider.GetRequiredService<IAppAuthorizationService>();
  }

  protected Guid CurrentDashboardId => _currentDashboardId ??= User.GetDashboardId()!.Value;
}