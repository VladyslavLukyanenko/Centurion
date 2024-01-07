using Microsoft.AspNetCore.Authorization;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Authorization;

public class AdminOrSameUserAuthorizationHandler : AuthorizationHandler<AdminOrSameUserRequirement>
{
  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
    AdminOrSameUserRequirement requirement)
  {
    if (context.User.HasAdminRole() || context.User.GetUserId() == requirement.UserId)
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}