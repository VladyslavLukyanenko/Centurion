namespace Centurion.Accounts.Core.Products.Services;

public class DashboardManager : IDashboardManager
{
  private readonly IDashboardRepository _dashboardRepository;

  public DashboardManager(IDashboardRepository dashboardRepository)
  {
    _dashboardRepository = dashboardRepository;
  }

  public async ValueTask<bool> TryJoinAsync(Guid dashboardId, long userId, CancellationToken ct = default)
  {
    if (await _dashboardRepository.AlreadyJoinedAsync(dashboardId, userId, ct))
    {
      return false;
    }

    await _dashboardRepository.AddMemberAsync(new Member(dashboardId, userId), ct);
    return true;
  }
}