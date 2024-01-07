using Centurion.Accounts.App.Services.Discord;
using Centurion.Accounts.Core;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.App.Products.Services;

public class UserProfileService : IUserProfileService
{
  private readonly IUserRepository _userRepository;
  private readonly IDiscordClient _discordClient;
  private readonly IDashboardRepository _dashboardRepository;

  public UserProfileService(IUserRepository userRepository, IDiscordClient discordClient,
    IDashboardRepository dashboardRepository)
  {
    _userRepository = userRepository;
    _discordClient = discordClient;
    _dashboardRepository = dashboardRepository;
  }

  public async ValueTask RefreshUserProfileIfOutdatedAsync(long userId, Guid dashboardId,
    CancellationToken ct = default)
  {
    await RefreshUserProfileIfOutdatedAsync(userId,
      async () => await _dashboardRepository.GetByIdAsync(dashboardId, ct), ct);
  }

  private async Task RefreshUserProfileIfOutdatedAsync(long userId, Func<ValueTask<Dashboard?>> dashboardProvider,
    CancellationToken ct)
  {
    User? user = await _userRepository.GetByIdAsync(userId, ct);
    if (user == null || !user.IsProfileInfoOutdated())
    {
      return;
    }

    var dashboard = await dashboardProvider();
    if (dashboard == null)
    {
      throw new CoreException("Dashboard not found");
    }

    var profile =
      await _discordClient.GetProfileByIdAsync(user.DiscordId, dashboard.DiscordConfig.BotAccessToken!, ct);
    if (profile == null)
    {
      throw new CoreException("Profile not found for user " + user.DiscordId);
    }

    user.UpdateProfile(profile.Username, User.GetAvatarUrl(profile.Id, profile.Avatar), profile.Discriminator);

    var cfg = dashboard.DiscordConfig;
    if (!cfg.IsBotCredentialsEmpty())
    {
      var discordMember = await _discordClient.GetGuildMemberAsync(cfg, profile.Id, ct);
      if (discordMember != null)
      {
        var roles = await _discordClient.GetGuildRolesAsync(cfg, ct);
        user.ReplaceDiscordRoles(roles.Where(r => discordMember.Roles.Contains(r.Id))
          .Select(r => new DiscordRoleInfo
          {
            Id = r.Id,
            Name = r.Name,
            ColorHex = r.GetHexColor()
          }));
      }
    }

    _userRepository.Update(user);
  }

  public async ValueTask RefreshUserProfileIfOutdatedAsync(long userId, Dashboard dashboard,
    CancellationToken ct = default) =>
    await RefreshUserProfileIfOutdatedAsync(userId, () => new ValueTask<Dashboard?>(dashboard), ct);
}