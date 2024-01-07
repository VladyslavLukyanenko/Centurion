using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Primitives;
using Centurion.Accounts.Core.Security;
using Centurion.Accounts.Core.Security.Services;
using Centurion.Accounts.Infra.Repositories;

namespace Centurion.Accounts.Infra.Security.Services;

public class EfUserMemberRoleBindingRepository : EfCrudRepository<UserMemberRoleBinding>,
  IUserMemberRoleBindingRepository
{
  public EfUserMemberRoleBindingRepository(DbContext context, IUnitOfWork unitOfWork)
    : base(context, unitOfWork)
  {
  }

  public async ValueTask<UserMemberRoleBinding?> GetUserRoleBindingAsync(long userId, long memberRoleId,
    CancellationToken ct = default)
  {
    return await DataSource.FirstOrDefaultAsync(_ => _.UserId == userId && _.MemberRoleId == memberRoleId, ct);
  }

  public async ValueTask<IList<UserMemberRoleBinding>> GetBindingsAsync(Guid dashboardId, long memberRoleId,
    CancellationToken ct = default)
  {
    return await DataSource.Where(_ => _.DashboardId == dashboardId && _.MemberRoleId == memberRoleId)
      .ToListAsync(ct);
  }
}