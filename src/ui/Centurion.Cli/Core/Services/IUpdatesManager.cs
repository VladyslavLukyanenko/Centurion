namespace Centurion.Cli.Core.Services;

public interface IUpdatesManager : IAppBackgroundWorker
{
  IObservable<Version> AvailableVersion { get; }
  IObservable<bool> NextVersionAvailable { get; }
  Task<Version> CheckForUpdatesAsync(CancellationToken ct = default);
}