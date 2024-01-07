namespace Centurion.Cli.Core;

public class DelegatingDisposable : IDisposable
{
  private readonly Action _disposeImpl;

  public DelegatingDisposable(Action disposeImpl)
  {
    _disposeImpl = disposeImpl;
  }

  public void Dispose() => _disposeImpl();
}