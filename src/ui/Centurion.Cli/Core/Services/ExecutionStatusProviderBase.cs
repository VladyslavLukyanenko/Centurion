using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Centurion.Cli.Core.Services;

public abstract class ExecutionStatusProviderBase : IExecutionStatusProvider
{
  private readonly BehaviorSubject<bool> _isFetching = new(false);

  protected ISubject<bool> FetchingTracker => _isFetching;

  public IObservable<bool> IsFetching => FetchingTracker.AsObservable();
}