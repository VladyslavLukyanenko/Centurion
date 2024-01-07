using NodaTime;

namespace Centurion.CloudManager.Web.Services;

public class LifetimeConfig
{
  public Duration KeepAlive { get; set; }
  public Duration LostConnection { get; set; }
  public Duration PendingTermination { get; set; }
  public Duration CleanupIdle { get; set; }
  public Duration PendingShutDown { get; set; }
}