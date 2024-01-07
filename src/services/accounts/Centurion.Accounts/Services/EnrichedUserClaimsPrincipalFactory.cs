using System.Globalization;
using System.Security.Claims;
using Centurion.Accounts.Foundation.Authorization;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.Services;

public class EnrichedUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
{
  private readonly IDashboardRepository _dashboardRepository;

  public EnrichedUserClaimsPrincipalFactory(UserManager<User> userManager, RoleManager<Role> roleManager,
    IOptions<IdentityOptions> options, IDashboardRepository dashboardRepository)
    : base(userManager, roleManager, options)
  {
    _dashboardRepository = dashboardRepository;
  }

  protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
  {
    var identity = await base.GenerateClaimsAsync(user);
    TryRemoveClaim(identity, JwtClaimTypes.Name);

    identity.AddClaim(new Claim(JwtClaimTypes.Id, user.Id.ToString(CultureInfo.InvariantCulture)));
    identity.AddClaim(new Claim(JwtClaimTypes.Subject, user.Id.ToString(CultureInfo.InvariantCulture)));
    identity.AddClaim(new Claim(JwtClaimTypes.Email, user.Email.Value));
    identity.AddClaim(new Claim(JwtClaimTypes.EmailVerified, user.Email.IsConfirmed.ToString().ToLowerInvariant()));
    identity.AddClaim(new Claim(JwtClaimTypes.Name, user.Name));

    identity.AddClaim(new Claim(AppClaimNames.DiscordId, user.DiscordId.ToString()));
    var accessibleDashboards = await _dashboardRepository.GetAccessibleDashboardsAsync(user.Id);
    foreach (var d in accessibleDashboards)
    {
      var claimName = d.IsProperty ? AppClaimNames.OwnDashboardId : AppClaimNames.JoinedDashboardId;
      identity.AddClaim(new Claim(claimName, d.Id.ToString()));
    }

    identity.AddClaims(user.DiscordRoles.Select(role => new Claim(AppClaimNames.DiscordRoleName, role.Name)));

    return identity;
  }

  private static void TryRemoveClaim(ClaimsIdentity identity, string claimToRemove)
  {
    var oldNameClaim = identity.FindFirst(claimToRemove);
    if (oldNameClaim != null)
    {
      identity.RemoveClaim(oldNameClaim);
    }
  }
}