using System.Globalization;
using System.Security.Claims;
using Centurion.Accounts.App;
using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Model.Discord;
using Centurion.Accounts.App.Security.Services;
using Centurion.Accounts.App.Services;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Services;

public class ClaimsProvider : IClaimsProvider
{
  private readonly IPathsService _pathsService;
  private readonly IConfiguration _configuration;
  private readonly IMemberRoleProvider _memberRoleProvider;
  private readonly IPermissionProvider _permissionProvider;

  public ClaimsProvider(IPathsService pathsService, IConfiguration configuration,
    IMemberRoleProvider memberRoleProvider, IPermissionProvider permissionProvider)
  {
    _pathsService = pathsService;
    _configuration = configuration;
    _memberRoleProvider = memberRoleProvider;
    _permissionProvider = permissionProvider;
  }

  public IList<Claim> GetDashboardClaims(Dashboard dashboard)
  {
    return new List<Claim>
    {
      new(AppClaimNames.CurrentDashboardId, dashboard.Id.ToString()),
      new(AppClaimNames.CurrentDashboardHostingMode, dashboard.HostingConfig.Mode.ToString()),
      new(AppClaimNames.CurrentDashboardDomain, dashboard.HostingConfig.DomainName),
      new(AppClaimNames.ProductVersion, dashboard!.ProductInfo.Version.ToString())
    };
  }

  public IList<Claim> GetUserClaims(User user)
  {
    var avatar = _pathsService.GetAbsoluteImageUrl(user.Avatar, _configuration[UserPicKeys.FallbackAvatarKey]);
    var claims = new List<Claim>
    {
      new(AppClaimNames.DiscordAvatar, avatar ?? ""),
      new(AppClaimNames.DiscordDiscriminator, user.Discriminator),
      new(AppClaimNames.DiscordRefreshedAt, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        .ToString(CultureInfo.InvariantCulture)),
    };

    return claims;
  }

  public async ValueTask<IList<Claim>> GetPermissionClaimsAsync(User user, Dashboard dashboard)
  {
    IEnumerable<string> permissions;
    var claims = new List<Claim>();
    if (dashboard.OwnerId != user.Id)
    {
      var roles = await _memberRoleProvider.GetRolesAsync(dashboard.Id, user.Id);
      foreach (var role in roles)
      {
        claims.Add(new(AppClaimNames.RoleId, role.MemberRoleId.ToString()));
        claims.Add(new(AppClaimNames.RoleName, role.RoleName));
      }

      permissions = roles.SelectMany(_ => _.Permissions)
        .Distinct();
    }
    else
    {
      permissions = _permissionProvider.GetSupportedPermissions().Select(_ => _.Permission);
    }

    return permissions.Select(p => new Claim(AppClaimNames.Permission, p)).ToList();
  }

  public IList<Claim> GetSecurityTokenClaims(SecurityToken token)
  {
    return new List<Claim>
    {
      new(AppClaimNames.DiscordExpiresIn, token.ExpiresIn.ToString(CultureInfo.InvariantCulture)),
      new(AppClaimNames.DiscordRefreshedAt, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        .ToString(CultureInfo.InvariantCulture)),
      new(AppClaimNames.DiscordAccessTokenToken, token.AccessToken),
      new(AppClaimNames.DiscordRefreshTokenToken, token.RefreshToken),
    };
  }
}