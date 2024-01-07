using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core.Products.Services;

public interface IDashboardRepository : ICrudRepository<Dashboard, Guid>
{
  ValueTask<Dashboard?> GetByRawLocationAsync(string rawLocation, CancellationToken ct = default);

  ValueTask<bool> AlreadyJoinedAsync(Guid dashboardId, long userId, CancellationToken ct = default);
  ValueTask<Member> AddMemberAsync(Member member, CancellationToken ct = default);
  ValueTask<IList<AccessibleDashboard>> GetAccessibleDashboardsAsync(long userId, CancellationToken ct = default);
  ValueTask<Dashboard?> GetByOwnerIdAsync(long userId, CancellationToken ct = default);
}