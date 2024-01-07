namespace Centurion.Cli.Core.Services;

public class OneclickAutomationStatus
{
  private OneclickAutomationStatus(string message, bool isError)
  {
    Message = message;
    IsError = isError;
  }

  public static implicit operator OneclickAutomationStatus(string status) => new(status, false);

  public static OneclickAutomationStatus Error(string message) => new(message, true);

  public string Message { get; }
  public bool IsError { get; }
}