using Centurion.Accounts.App.Identity.Model;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.App.Identity.Services;

public interface IUserRefProvider
{
  ValueTask<UserRef> GetRefAsync(long userId, CancellationToken ct = default);
  ValueTask<IDictionary<long, UserRef>> GetRefsAsync(IEnumerable<long> userIds, CancellationToken ct = default);
  ValueTask<IPagedList<UserRef>> GetRefsPageAsync(UserRefsPageRequest pageRequest, CancellationToken ct);
}