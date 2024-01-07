using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Identity.Services;

public interface IUserRepository : ICrudRepository<User>
{
  Task<IList<string>> GetRolesAsync(long userId, CancellationToken ct = default);
  Task<User> GetByEmailAsync(string subject, CancellationToken ct = default);
  Task<User> GetByDiscordIdAsync(ulong discordId, CancellationToken ct = default);
}