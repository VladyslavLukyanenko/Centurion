using System.Reactive.Disposables;

namespace Centurion.CloudManager;

internal static class DisposableExtensions
{
  public static void DisposeWith(this IDisposable self, CompositeDisposable container) => container.Add(self);
}