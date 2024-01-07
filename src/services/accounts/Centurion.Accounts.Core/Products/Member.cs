using NodaTime;

namespace Centurion.Accounts.Core.Products;

public class Member
{
  private Member()
  {
  }

  public Member(Guid dashboardId, long userId)
  {
    DashboardId = dashboardId;
    UserId = userId;
  }

  public Guid DashboardId { get; private set; }
  public long UserId { get; private set; }
  public Instant JoinedAt { get; private set; } = SystemClock.Instance.GetCurrentInstant();
}