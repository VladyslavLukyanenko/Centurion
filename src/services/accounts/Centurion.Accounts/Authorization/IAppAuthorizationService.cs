using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Authorization;

public interface IAppAuthorizationService
{
  ValueTask<AuthorizationResult> AdminOrMemberAsync(Guid dashboardId);
  ValueTask<AuthorizationResult> AdminOrSameUserAsync(long userId);
  // ValueTask<AuthorizationResult> AdminOrDashboardOwnerAsync(Guid dashboardId);
  ValueTask<AuthorizationResult> AuthorizeCurrentPermissionsAsync(Guid dashboardId);
}