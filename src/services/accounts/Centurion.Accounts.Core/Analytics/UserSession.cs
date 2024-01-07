using System.Net;
using Centurion.Accounts.Core.Products;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.Accounts.Core.Analytics;

public class UserSession : Primitives.Entity<Guid>, IDashboardBoundEntity
{
  private static readonly Duration SessionTimeout = Duration.FromMinutes(5);

  private UserSession()
  {
  }

  public UserSession(Guid dashboardId, long? userId, string userAgent, IPAddress? ipAddress)
  {
    DashboardId = dashboardId;
    UserId = userId;
    UserAgent = userAgent;
    IpAddress = ipAddress;

    StartedAt = SystemClock.Instance.GetCurrentInstant();
    LastActivityAt = StartedAt;
  }

  private static bool IsAddressLoopback(IPAddress? ip) => ip != null && IPAddress.IsLoopback(ip);

  public Result Refresh(string userAgent, IPAddress? ip)
  {
    if (!string.Equals(userAgent, UserAgent, StringComparison.OrdinalIgnoreCase)
        || Equals(IpAddress, ip) == false && IsAddressLoopback(IpAddress) && !IsAddressLoopback(ip))
    {
      return Result.Failure("Invalid user-agent or ip address");
    }

    var now = SystemClock.Instance.GetCurrentInstant();
    if (!UserId.HasValue && now - LastActivityAt > SessionTimeout)
    {
      return Result.Failure("Session timed out");
    }

    LastActivityAt = now;
    return Result.Success();
  }

  public Instant StartedAt { get; private set; }
  public Instant LastActivityAt { get; private set; }
  public Guid DashboardId { get; private set; }
  public long? UserId { get; private set; }
  public string UserAgent { get; private set; } = null!;
  public IPAddress? IpAddress { get; private set; }
}