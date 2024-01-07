using Microsoft.AspNetCore.Authorization;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Authorization;

public class AdminOrMemberAuthorizationHandler : AuthorizationHandler<AdminOrMemberRequirement>
{
  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
    AdminOrMemberRequirement requirement)
  {
    if (context.User.HasAdminRole() || context.User.OwnsDashboard(requirement.DashboardId)
                                    || context.User.JoinedDashboard(requirement.DashboardId))
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}