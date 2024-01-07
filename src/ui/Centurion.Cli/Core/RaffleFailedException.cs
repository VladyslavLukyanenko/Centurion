namespace Centurion.Cli.Core;

public class RaffleFailedException : InvalidOperationException
{
  public RaffleFailedException(string rootCause, string message)
    : base(message)
  {
    RootCause = rootCause;
  }

  public string RootCause { get; private set; }
}