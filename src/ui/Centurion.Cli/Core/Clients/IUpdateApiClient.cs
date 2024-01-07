namespace Centurion.Cli.Core.Clients;

public interface IUpdateApiClient
{
  Task<Version> GetLatestAvailableVersionAsync(CancellationToken ct = default);

  Task DownloadInstallerAsync(Stream output, Version version, ProgressChanged onProgressCallback,
    CancellationToken ct = default);
}