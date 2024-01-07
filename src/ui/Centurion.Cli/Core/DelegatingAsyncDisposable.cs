namespace Centurion.Cli.Core;

public class DelegatingAsyncDisposable : IAsyncDisposable
{
  private readonly Func<ValueTask> _disposeImpl;

  public DelegatingAsyncDisposable(Func<ValueTask> disposeImpl)
  {
    _disposeImpl = disposeImpl;
  }

  public async ValueTask DisposeAsync() => await _disposeImpl();
}