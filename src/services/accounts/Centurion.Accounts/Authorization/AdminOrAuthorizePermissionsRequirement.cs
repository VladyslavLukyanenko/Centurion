using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Authorization;

public class AdminOrAuthorizePermissionsRequirement : IAuthorizationRequirement
{
  public AdminOrAuthorizePermissionsRequirement(IReadOnlyList<string> requiredPermissions, Guid dashboardId)
  {
    RequiredPermissions = requiredPermissions;
    DashboardId = dashboardId;
  }

  public IReadOnlyList<string> RequiredPermissions { get; }
  public Guid DashboardId { get; }
}