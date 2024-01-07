using Centurion.Accounts.App.Model.Discord;
using Centurion.Accounts.Core.Products;

namespace Centurion.Accounts.App.Services.Discord;

public interface IDiscordClient
{
  ValueTask<SecurityToken?> AuthenticateAsync(DiscordOAuthConfig config, string code,
    CancellationToken ct = default);

  ValueTask<SecurityToken?> ReauthenticateAsync(DiscordOAuthConfig config, string refreshToken,
    CancellationToken ct = default);

  ValueTask<DiscordUser?> GetProfileAsync(string accessToken, CancellationToken ct = default);

  ValueTask<bool> JoinGuildAsync(DiscordConfig config, string accessToken, DiscordUser user,
    CancellationToken ct = default);

  ValueTask<DiscordUser?> GetProfileByIdAsync(ulong discordId, string accessToken, CancellationToken ct = default);

  ValueTask<GuildMember?> GetGuildMemberAsync(DiscordConfig config, ulong discordUserId,
    CancellationToken ct = default);

  ValueTask<DiscordRole[]> GetGuildRolesAsync(DiscordConfig config, CancellationToken ct = default);
}