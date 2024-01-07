using NodaTime;

namespace Centurion.TaskManager.Core.Services;

public interface ICloudConnection : IDisposable
{
  string Id { get; }
  Instant ConnectedAt { get; }
  string? DnsName { get; }
  string UserId { get; }
  event EventHandler Closed;
}