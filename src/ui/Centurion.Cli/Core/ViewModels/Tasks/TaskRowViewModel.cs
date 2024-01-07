using System.Reactive.Disposables;
using System.Reactive.Linq;
using Centurion.Cli.Core.Composition;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Services.Tasks;
using Centurion.Contracts;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;

namespace Centurion.Cli.Core.ViewModels.Tasks;

[TransientViewModel]
public class TaskRowViewModel : ViewModelBase
{
  private readonly IProfilesRepository _profiles;
  private readonly IProxyGroupsRepository _proxies;
  private readonly ITaskStatusRegistry _statuses;
  private CheckoutTaskModel _task = null!;

  public TaskRowViewModel(IProfilesRepository profiles, IProxyGroupsRepository proxies, ITaskStatusRegistry statuses)
  {
    _profiles = profiles;
    _proxies = proxies;
    _statuses = statuses;
    //
    // proxies.Items.WatchValue(task.SessionId.GetValueOrDefault())
    //   .Select(_ => _?.Name ?? "<None>")
    //   .ToPropertyEx(this, _ => _.SessionName)
    //   .DisposeWith(Disposable);

    TaskChanged = this.WhenAnyValue(_ => _.DisplayStatus)
      .DistinctUntilChanged()
      .Select(_ => this);
  }

  public static TaskRowViewModel Create(CheckoutTaskModel task, IReadonlyDependencyResolver resolver)
  {
    return resolver.GetService<TaskRowViewModel>()!
      .InitializeWith(task);
  }

  private TaskRowViewModel InitializeWith(CheckoutTaskModel task)
  {
    _task = task;

    Observable.Return(TaskStatusData.Idle)
      .Concat(_statuses.Statuses.WatchValue(task.Id)
        .Select(_ => _.Status))
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(s =>
      {
        Status = s;
        Progress = s.ToProgress();
      })
      .DisposeWith(Disposable);

    var status = this.WhenAnyValue(_ => _.Status)
      .Replay(1)
      .RefCount();

    status
      .Select(_ => _.AsString())
      .DistinctUntilChanged()
      // .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.DisplayStatus)
      .DisposeWith(Disposable);

    status.Select(s => s.IsRunning())
      .DistinctUntilChanged()
      .ToPropertyEx(this, _ => _.IsRunning)
      .DisposeWith(Disposable);
    //
    // sessions.Items.WatchValue(task.SessionId.GetValueOrDefault())
    //   .ToPropertyEx(this, _ => _.Session)
    //   .DisposeWith(Disposable);


    // profiles.Items.WatchValue(task.ProfileIds)
    //   .ToPropertyEx(this, _ => _.Profiles)
    //   .DisposeWith(Disposable);
    if (task.ProfileIds.Count > 1)
    {
      var profile = _profiles.Items.Items
        .FirstOrDefault(g => g.Profiles.Any(p => task.ProfileIds.Contains(p.Id)));

      const string fallbackProfileGroupName = "GROUP_NOT_FOUND";
      ProfileName = $"{profile?.Name ?? fallbackProfileGroupName} ({task.ProfileIds.Count})";
    }
    else
    {
      _profiles.Items
        .Connect()
        .TransformMany(_ => _.Profiles, _ => _.Id)
        .WatchValue(task.ProfileIds.FirstOrDefault())
        .Select(_ => _.Name)
        .DistinctUntilChanged()
        .Subscribe(n => ProfileName = n)
        .DisposeWith(Disposable);
    }

    //
    // if (task.CheckoutProxyPoolId.HasValue)
    // {
    //   proxies.Items.WatchValue(task.CheckoutProxyPoolId.Value)
    //   .Select(_ => _.Name)
    //   .ToPropertyEx(this, _ => _.CheckoutProxyName)
    //   .DisposeWith(Disposable);
    // }

    // todo: use prop change tracking + switch
    _proxies.Items.WatchValue(task.CheckoutProxyPoolId.GetValueOrDefault())
      .DistinctUntilChanged()
      .ToPropertyEx(this, _ => _.CheckoutProxies)
      .DisposeWith(Disposable);

    _proxies.Items.WatchValue(task.MonitorProxyPoolId.GetValueOrDefault())
      .DistinctUntilChanged()
      .ToPropertyEx(this, _ => _.MonitorProxies)
      .DisposeWith(Disposable);

    _proxies.Items.WatchValue(task.CheckoutProxyPoolId.GetValueOrDefault())
      .Select(_ => _?.Name ?? "<None>")
      .DistinctUntilChanged()
      .ToPropertyEx(this, _ => _.CheckoutProxyName)
      .DisposeWith(Disposable);

    _proxies.Items.WatchValue(task.MonitorProxyPoolId.GetValueOrDefault())
      .Select(_ => _?.Name ?? "<None>")
      .DistinctUntilChanged()
      .ToPropertyEx(this, _ => _.MonitorProxyName)
      .DisposeWith(Disposable);

    return this;
  }

  public ProxyGroup? CheckoutProxies { [ObservableAsProperty] get; }

  public ProxyGroup? MonitorProxies { [ObservableAsProperty] get; }

  // public SessionModel? Session { [ObservableAsProperty] get; }
  // public IList<ProfileModel> Profiles { get; private set; } = new List<ProfileModel>();
  public CheckoutTaskModel Task => _task;
  [Reactive] public string ProfileName { get; private set; } = null!;
  [Reactive] public TaskProgress Progress { get; private set; }
  public Module Module => _task.Module;
  public string? CheckoutProxyName { [ObservableAsProperty] get; }
  public string? MonitorProxyName { [ObservableAsProperty] get; }
  public string? SessionName { [ObservableAsProperty] get; }
  public IObservable<TaskRowViewModel> TaskChanged { get; }
  public string DisplayStatus { [ObservableAsProperty] get; } = TaskStatusData.Idle.AsString();
  [Reactive] public TaskStatusData Status { get; private set; } = TaskStatusData.Idle;
  public bool IsRunning { [ObservableAsProperty] get; }
}