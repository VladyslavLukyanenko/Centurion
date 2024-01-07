using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Security.Services;

public interface IUserMemberRoleBindingRepository : ICrudRepository<UserMemberRoleBinding>
{
  ValueTask<UserMemberRoleBinding?> GetUserRoleBindingAsync(long userId, long memberRoleId,
    CancellationToken ct = default);

  ValueTask<IList<UserMemberRoleBinding>> GetBindingsAsync(Guid dashboardId, long memberRoleId,
    CancellationToken ct = default);
}