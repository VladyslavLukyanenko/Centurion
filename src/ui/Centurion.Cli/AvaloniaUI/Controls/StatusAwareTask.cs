using System.Reactive.Disposables;
using System.Reactive.Linq;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Contracts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class StatusAwareTask : ReactiveObject, IDisposable
{
  private readonly CompositeDisposable _disposable = new();

  public StatusAwareTask(IObservable<TaskStatusData> statusChanges, CheckoutTaskModel task)
  {
    Observable.Return(TaskStatusData.Idle)
      .Concat(statusChanges)
      .Subscribe(s => Status = s)
      .DisposeWith(_disposable);
    Task = task;
  }

  [Reactive] public TaskStatusData Status { get; private set; } = TaskStatusData.Idle;
  public CheckoutTaskModel Task { get; private set; }

  public void Dispose()
  {
    _disposable.Dispose();
  }
}