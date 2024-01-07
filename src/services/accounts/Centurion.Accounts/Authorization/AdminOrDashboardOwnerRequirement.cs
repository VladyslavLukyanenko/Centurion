using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Authorization;

public class AdminOrDashboardOwnerRequirement : IAuthorizationRequirement
{
  public AdminOrDashboardOwnerRequirement(Guid dashboardId)
  {
    DashboardId = dashboardId;
  }

  public Guid DashboardId { get; }
}