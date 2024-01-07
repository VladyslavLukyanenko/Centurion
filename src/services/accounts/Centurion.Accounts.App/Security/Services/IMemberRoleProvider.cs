using Centurion.Accounts.App.Security.Model;

namespace Centurion.Accounts.App.Security.Services;

public interface IMemberRoleProvider
{
  ValueTask<IList<MemberRoleData>> GetMemberRolesAsync(Guid dashboardId, CancellationToken ct = default);
  ValueTask<IList<BoundMemberRoleData>> GetRolesAsync(Guid dashboardId, long userId, CancellationToken ct = default);
}