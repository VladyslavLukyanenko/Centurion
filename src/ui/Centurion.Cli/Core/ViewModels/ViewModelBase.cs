using System.Reactive.Disposables;
using ReactiveUI;

namespace Centurion.Cli.Core.ViewModels;

public class ViewModelBase : ReactiveObject, IDisposable
{
  protected readonly CompositeDisposable Disposable = new();

  ~ViewModelBase()
  {
    Disposable.Dispose();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (disposing)
    {
      if (!Disposable.IsDisposed)
      {
        Disposable.Dispose();
        Disposed?.Invoke(this, EventArgs.Empty);
      }
    }
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  public event EventHandler? Disposed;
}