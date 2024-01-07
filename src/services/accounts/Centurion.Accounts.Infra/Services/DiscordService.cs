using Discord;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using IDiscordClient = Centurion.Accounts.App.Services.Discord.IDiscordClient;

namespace Centurion.Accounts.Infra.Services;

public class DiscordService : IDiscordService
{
  private readonly IDiscordClient _discordClient;
  private readonly IDiscordClientProvider _discordClientProvider;
  private readonly IUserRepository _userRepository;

  public DiscordService(IDiscordClient discordClient,
    IDiscordClientProvider discordClientProvider, IUserRepository userRepository)
  {
    _discordClient = discordClient;
    _discordClientProvider = discordClientProvider;
    _userRepository = userRepository;
  }

  public async ValueTask JoinToGuildAsync(Dashboard dashboard, string discordToken, CancellationToken ct = default)
  {
    var discordUser = await _discordClient.GetProfileAsync(discordToken, ct);
    await _discordClient.JoinGuildAsync(dashboard.DiscordConfig, discordToken, discordUser!, ct);
  }

  public async ValueTask RemoveRolesByKeyAsync(Dashboard dashboard, LicenseKey key, Plan plan,
    CancellationToken ct = default)
  {
    var config = dashboard.DiscordConfig;
    var client = await _discordClientProvider.GetInitializedClientAsync(key.DashboardId, ct);
    var guild = client.GetGuild(config!.GuildId);

    IEnumerable<IRole> roles = new IRole[] {guild.GetRole(config.RoleId), guild.GetRole(plan.DiscordRoleId)};

    if (key.UserId.HasValue)
    {
      var user = await _userRepository.GetByIdAsync(key.UserId.Value, ct);
      var guildUser = guild.GetUser(user!.DiscordId);
      await guildUser.RemoveRolesAsync(roles);
    }
  }
}