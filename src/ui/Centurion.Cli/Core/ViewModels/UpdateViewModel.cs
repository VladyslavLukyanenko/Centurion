using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using Centurion.Cli.Core.Clients;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.ToastNotifications;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels;

public class UpdateViewModel : ViewModelBase
{
  private static readonly string InstallerExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ".pkg";

  private readonly IUpdateApiClient _updateApiClient;
  private string? _installerDirectory;
  private CancellationTokenSource? _cts;
  private readonly IToastNotificationManager _toasts;
  private readonly ILogger<UpdateViewModel> _logger;

#if DEBUG
  public UpdateViewModel()
  {
  }
#endif

  public UpdateViewModel(IUpdatesManager updatesManager, IUpdateApiClient updateApiClient,
    IToastNotificationManager toasts, ILogger<UpdateViewModel> logger)
  {
    _updateApiClient = updateApiClient;
    _toasts = toasts;
    _logger = logger;
    CheckForUpdatesCommand = ReactiveCommand.CreateFromTask(async () =>
    {
      SkippedVersion = null;
      return await updatesManager.CheckForUpdatesAsync() > AppInfo.CurrentAppVersion;
    });
    PrepareToUpdateCommand = ReactiveCommand.CreateFromTask(PrepareAsync);
    LaunchUpdaterCommand = ReactiveCommand.Create(LaunchUpdaterAsync);

    this.WhenAnyValue(_ => _.SkippedVersion)
      .Select(v => v != null && v <= NextVersion)
      .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.IsAvailableVersionSkipped);

    updatesManager.AvailableVersion
      .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.NextVersion);

    this.WhenAnyValue(_ => _.NextVersion)
      .Select(nextVersion => nextVersion > AppInfo.CurrentAppVersion)
      .ObserveOn(RxApp.MainThreadScheduler)
      .ToPropertyEx(this, _ => _.IsUpdateAvailable);

    CancelDownloadingCommand = ReactiveCommand.Create(() =>
    {
      if (PreventCancellation)
      {
        return;
      }

      SkippedVersion = NextVersion;
      if (IsInProgress)
      {
        _cts?.Cancel();
      }
    });

    Progress = -1;
  }

  [Reactive] public int DownloadedMb { get; private set; }
  [Reactive] public int TotalSizeMb { get; private set; }

  [Reactive] public bool IsInProgress { get; private set; }

  public ReactiveCommand<Unit, Unit> CancelDownloadingCommand { get; }


  public bool IsUpdateAvailable { [ObservableAsProperty] get; }
  public bool IsAvailableVersionSkipped { [ObservableAsProperty] get; }

  public Version? NextVersion { [ObservableAsProperty] get; }
  [Reactive] public Version? SkippedVersion { get; private set; }
  [Reactive] public int Progress { get; set; }
  [Reactive] public bool PreventCancellation { get; set; }

  public ReactiveCommand<Unit, bool> CheckForUpdatesCommand { get; }

  public ReactiveCommand<Unit, Unit> PrepareToUpdateCommand { get; }

  public ReactiveCommand<Unit, Unit> LaunchUpdaterCommand { get; }


  private async Task PrepareAsync()
  {
    if (!IsUpdateAvailable)
    {
      _toasts.Show(ToastContent.Warning("No updates available"));
      return;
    }

    if (IsInProgress)
    {
      _toasts.Show(ToastContent.Information("Update already running"));
      return;
    }

    _cts = new CancellationTokenSource();

    _installerDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(_installerDirectory);
    var installerFileName = NextVersion + InstallerExt;

    var installerFullPath = Path.Combine(_installerDirectory, installerFileName);
    try
    {
      IsInProgress = true;
      using (var file = File.Create(installerFullPath))
      {
        await _updateApiClient.DownloadInstallerAsync(file, NextVersion!,
          (totalBytes, downloadedBytes, calculatedProgressPercents) =>
          {
            TotalSizeMb = (int)(totalBytes / 1024 / 1024);
            DownloadedMb = (int)(downloadedBytes / 1024 / 1024);
            Progress = calculatedProgressPercents;
          }, _cts.Token);
      }

      LaunchUpdaterCommand.Execute().Subscribe();
    }
    catch (OperationCanceledException)
    {
      // expected
    }
    catch (IOException exc)
    {
      _logger.LogError(exc, "Error on downloading update");
      _toasts.Show(ToastContent.Error("An error occured on downloading update. Please try again later"));
    }
    finally
    {
      IsInProgress = false;
      PreventCancellation = false;
      Progress = 0;
    }
  }

  private void LaunchUpdaterAsync()
  {
    try
    {
      var installerFullPath = Path.Combine(_installerDirectory!, $"{NextVersion}" + InstallerExt);
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
      {
        Process.Start(installerFullPath);
      }
      else
      {
        Process.Start("open", installerFullPath);
        Thread.Sleep(1000);
      }
    }
    catch (Exception exc)
    {
      _logger.LogError(exc, "Error on starting installer");
      _toasts.Show(ToastContent.Error("An error occured on starting installer. Please try again later"));
    }
    finally
    {
      IsInProgress = false;
      PreventCancellation = false;
      Progress = 0;
    }
  }
}