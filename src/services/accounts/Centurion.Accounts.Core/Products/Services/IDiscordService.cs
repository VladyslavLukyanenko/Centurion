namespace Centurion.Accounts.Core.Products.Services;

public interface IDiscordService
{
  ValueTask JoinToGuildAsync(Dashboard dashboard, string discordToken, CancellationToken ct = default);
  ValueTask RemoveRolesByKeyAsync(Dashboard dashboard, LicenseKey key, Plan plan, CancellationToken ct = default);
}