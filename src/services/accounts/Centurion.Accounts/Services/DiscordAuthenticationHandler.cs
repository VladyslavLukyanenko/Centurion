using System.Security.Claims;
using Centurion.Accounts.App.Model.Discord;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.App.Services.Discord;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Services;

public class DiscordAuthenticationHandler : IDiscordAuthenticationHandler
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserManager _userManager;
  private readonly IUserRepository _userRepository;
  private readonly IDiscordClient _discordClient;
  private readonly IUserProfileService _userProfileService;
  private readonly IDashboardManager _dashboardManager;
  private readonly IClaimsProvider _claimsProvider;

  public DiscordAuthenticationHandler(IUnitOfWork unitOfWork, IUserManager userManager,
    IUserRepository userRepository, IDiscordClient discordClient, IUserProfileService userProfileService,
    IDashboardManager dashboardManager, IClaimsProvider claimsProvider)
  {
    _unitOfWork = unitOfWork;
    _userManager = userManager;
    _userRepository = userRepository;
    _discordClient = discordClient;
    _userProfileService = userProfileService;
    _dashboardManager = dashboardManager;
    _claimsProvider = claimsProvider;
  }

  public async ValueTask<Result<AuthenticationResult>> AuthenticateAsync(Dashboard dashboard,
    SecurityToken token, CancellationToken ct = default)
  {
    var profile = await _discordClient.GetProfileAsync(token.AccessToken, ct);
    if (profile == null)
    {
      return Result.Failure<AuthenticationResult>("Profile not found");
    }

    var user = await _userRepository.GetByDiscordIdAsync(profile.Id, ct);
    if (user == null)
    {
      user = User.CreateWithDiscordId(profile.Email, profile.Username, profile.Id, profile.Avatar,
        profile.Discriminator);

      var discordMember = await _discordClient.GetGuildMemberAsync(dashboard.DiscordConfig, profile.Id, ct);
      if (discordMember != null)
      {
        var roles = await _discordClient.GetGuildRolesAsync(dashboard.DiscordConfig, ct);
        user.ReplaceDiscordRoles(roles.Where(r => discordMember.Roles.Contains(r.Id))
          .Select(r => new DiscordRoleInfo
          {
            Id = r.Id,
            Name = r.Name,
            ColorHex = r.GetHexColor()
          }));
      }

      var error = await _userManager.CreateAsync(user, ct);
      if (!string.IsNullOrEmpty(error))
      {
        return Result.Failure<AuthenticationResult>(error);
      }
    }

    await _discordClient.JoinGuildAsync(dashboard.DiscordConfig, token.AccessToken, profile, ct);
    if (dashboard.OwnerId != user.Id)
    {
      await _dashboardManager.TryJoinAsync(dashboard.Id, user.Id, ct);
    }

    await _userProfileService.RefreshUserProfileIfOutdatedAsync(user.Id, dashboard, ct);
    var claims = await AddDefaultClaimsAsync(token, dashboard, user);
    await _unitOfWork.SaveEntitiesAsync(ct);

    return new AuthenticationResult
    {
      Claims = claims,
      AuthenticatedUser = user
    };
  }

  private async Task<List<Claim>> AddDefaultClaimsAsync(SecurityToken securityToken, Dashboard dashboard,
    User user)
  {
    var claims = new List<Claim>();
    claims.AddRange(_claimsProvider.GetDashboardClaims(dashboard));
    claims.AddRange(_claimsProvider.GetUserClaims(user));
    claims.AddRange(_claimsProvider.GetSecurityTokenClaims(securityToken));
    claims.AddRange(await _claimsProvider.GetPermissionClaimsAsync(user, dashboard));

    return claims;
  }
}