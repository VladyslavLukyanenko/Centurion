using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Authorization;

public class AdminOrMemberRequirement : IAuthorizationRequirement
{
  public AdminOrMemberRequirement(Guid dashboardId)
  {
    DashboardId = dashboardId;
  }

  public Guid DashboardId { get; }
}