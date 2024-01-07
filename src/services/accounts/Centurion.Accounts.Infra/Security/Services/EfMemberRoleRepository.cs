using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Security.Services;

public class EfMemberRoleRepository : EfCrudRepository<MemberRole>, IMemberRoleRepository
{
  public EfMemberRoleRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public async ValueTask<IList<MemberRole>> GetDashboardRolesByIdsAsync(Guid dashboardId, IEnumerable<long> roleIds,
    CancellationToken ct = default)
  {
    return await DataSource.Where(_ => _.DashboardId == dashboardId && roleIds.Contains(_.Id))
      .ToListAsync(ct);
  }
}