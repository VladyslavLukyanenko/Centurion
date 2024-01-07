using System.Net;

namespace Centurion.Accounts.Core.Analytics.Services;

public interface IUserSessionService
{
  ValueTask<Guid> RefreshOrCreateSessionAsync(Guid dashboardId, Guid? sessionId, string userAgent,
    IPAddress? ipAddress, long? userId, CancellationToken ct = default);
}