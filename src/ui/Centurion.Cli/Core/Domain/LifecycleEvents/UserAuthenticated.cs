namespace Centurion.Cli.Core.Domain.LifecycleEvents;

public class UserAuthenticated : ApplicationLifecycleEventBase
{
  public static UserAuthenticated Instance { get; } = new();
    
  private UserAuthenticated()
  {
  }
}