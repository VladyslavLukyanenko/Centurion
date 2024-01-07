using Microsoft.AspNetCore.Authorization;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Authorization;

public class AdminOrDashboardOwnerAuthorizationHandler : AuthorizationHandler<AdminOrDashboardOwnerRequirement>
{
  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
    AdminOrDashboardOwnerRequirement requirement)
  {
    if (context.User.HasAdminRole() || context.User.OwnsDashboard(requirement.DashboardId))
    {
      context.Succeed(requirement);
    }
      
    return Task.CompletedTask;
  }
}