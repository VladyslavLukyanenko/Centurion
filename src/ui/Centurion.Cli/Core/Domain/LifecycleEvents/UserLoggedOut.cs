namespace Centurion.Cli.Core.Domain.LifecycleEvents;

public class UserLoggedOut : ApplicationLifecycleEventBase
{
  public static UserLoggedOut Instance { get; } = new();
    
  private UserLoggedOut()
  {
  }
}