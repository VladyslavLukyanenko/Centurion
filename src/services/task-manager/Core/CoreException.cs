namespace Centurion.TaskManager.Core;

public class CoreException : Exception
{
  public CoreException()
  {
  }

  public CoreException(string message)
    : base(message)
  {
  }
}