using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Security.Services;

public interface IMemberRoleRepository : ICrudRepository<MemberRole>
{
  ValueTask<IList<MemberRole>> GetDashboardRolesByIdsAsync(Guid dashboardId, IEnumerable<long> roleIds,
    CancellationToken ct = default);
}