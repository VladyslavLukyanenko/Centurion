using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Centurion.Cli.Core.Clients;
using Centurion.Cli.Core.Services.ToastNotifications;
using ReactiveUI;

namespace Centurion.Cli.Core.Services;

public class UpdatesManager : IUpdatesManager
{
  private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

  private readonly IUpdateApiClient _updateApiClient;
  private readonly BehaviorSubject<Version> _nextVersion = new(AppInfo.CurrentAppVersion);

  public UpdatesManager(IUpdateApiClient updateApiClient, IToastNotificationManager toasts)
  {
    _updateApiClient = updateApiClient;
    AvailableVersion = _nextVersion;

    NextVersionAvailable = AvailableVersion.Select(v => v > AppInfo.CurrentAppVersion);
    NextVersionAvailable
      .DistinctUntilChanged()
      .Subscribe(available =>
      {
        if (available)
        {
          toasts.Show(ToastContent.Success(
            $"An updated version '{_nextVersion.Value}' is available. Would you like to update?"));
        }
      });
  }

  public IObservable<Version> AvailableVersion { get; }
  public IObservable<bool> NextVersionAvailable { get; }

  public async Task<Version> CheckForUpdatesAsync(CancellationToken ct = default)
  {
    var nextVersion =  await _updateApiClient.GetLatestAvailableVersionAsync(ct);
    _nextVersion.OnNext(nextVersion);

    return nextVersion;
  }

  public void Spawn()
  {
    Observable.Interval(CheckInterval, RxApp.TaskpoolScheduler)
      .Do(_ => CheckForUpdatesAsync().ToObservable())
      .Subscribe();
  }
}