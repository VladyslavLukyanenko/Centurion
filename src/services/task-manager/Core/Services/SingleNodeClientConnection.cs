using NodaTime;

namespace Centurion.TaskManager.Core.Services;

public class SingleNodeClientConnection : ICloudConnection
{
  private readonly CloudServiceConfig _config;

  public SingleNodeClientConnection(string userId, CloudServiceConfig config)
  {
    if (!config.UseSingleNode)
    {
      throw new InvalidOperationException("Can't use single node mode when its configured for distributed infra");
    }

    _config = config;
    UserId = userId;
    Id = Guid.NewGuid().ToString();
    ConnectedAt = SystemClock.Instance.GetCurrentInstant();
  }

  public void Dispose()
  {
    Closed?.Invoke(this, EventArgs.Empty);
  }

  public string Id { get; }
  public Instant ConnectedAt { get; }
  public string? DnsName => _config.CheckoutUrl;
  public string UserId { get; }
  public event EventHandler? Closed;
}