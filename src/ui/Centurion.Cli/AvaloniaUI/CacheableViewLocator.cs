using System.Collections.Concurrent;
using System.Reactive.Linq;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.ViewModels;
using CSharpFunctionalExtensions;
using ReactiveUI;

namespace Centurion.Cli.AvaloniaUI;

public class CacheableViewLocator : IViewLocator, IAppStateHolder
{
  private readonly IViewLocator _impl;
  private readonly ConcurrentDictionary<ViewModelBase, IViewFor> _viewsCache = new();

  public CacheableViewLocator(IViewLocator impl)
  {
    _impl = impl;
  }

  public IViewFor? ResolveView<T>(T? viewModel, string? contract = null)
  {
    if (viewModel is not ViewModelBase vm)
    {
      return _impl.ResolveView(viewModel, contract);
    }

    var view = _viewsCache.GetOrAdd(vm, static (_, ctx) => ctx.Self._impl.ResolveView(ctx.Vm, ctx.Contract)!,
      (Self: this, Vm: viewModel, Contract: contract));

    vm.Disposed += RemoveOnDisposed;

    return view;

    void RemoveOnDisposed(object? s, EventArgs e)
    {
      _viewsCache.Remove(vm, out _);
      vm.Disposed -= RemoveOnDisposed;
    }
  }

  public IObservable<bool> IsFetching { get; } = Observable.Return(false);
  public ValueTask<Result> InitializeAsync(CancellationToken ct = default) => default;

  public void ResetCache()
  {
    _viewsCache.Clear();
  }
}