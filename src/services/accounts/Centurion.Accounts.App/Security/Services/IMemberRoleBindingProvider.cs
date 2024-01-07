using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Security.Services;

public interface IMemberRoleBindingProvider
{
  ValueTask<IPagedList<StaffMemberData>> GetMembersPageAsync(Guid dashboardId, StaffMemberPageRequest pageRequest,
    CancellationToken ct);

  ValueTask<MemberSummaryData?> GetSummaryAsync(long userId, Guid dashboardId, CancellationToken ct = default);
  ValueTask<IList<StaffRoleMembersData>> GetRolesAsync(Guid dashboardId, CancellationToken ct = default);
}