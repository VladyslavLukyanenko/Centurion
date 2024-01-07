using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Products.Services;

public interface IPlanRepository : ICrudRepository<Plan>
{
  ValueTask<(ulong GuidId, IEnumerable<ulong> RoleIds)> GetDiscordRolesInfoAsync(long planId,
    CancellationToken ct = default);

  ValueTask<Plan?> GetByPasswordAsync(Guid dashboardId, string password, CancellationToken ct = default);
}