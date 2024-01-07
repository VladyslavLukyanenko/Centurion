using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Centurion.Accounts.Foundation.Model;

namespace Centurion.Cli.Core.Clients;

public class HttpClientUpdateApiClient : IUpdateApiClient
{
  private readonly HttpClient _client;
  private static readonly Version NullVersion = new(0, 0, 0, 0);
  private const int BufferSize = 4096;
  private static readonly string TargetOs = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" : "osx";
  private static readonly string InstallerExt = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ".pkg";

  public HttpClientUpdateApiClient(HttpClient client)
  {
    _client = client;
  }

  public async Task<Version> GetLatestAvailableVersionAsync(CancellationToken ct = default)
  {
    var r = await _client.GetAsync($"/v1/updates/stable/{TargetOs}/{Arch}", ct);
    if (!r.IsSuccessStatusCode)
    {
      return NullVersion;
    }

    var response = await r.Content.ReadFromJsonAsync<ApiContract<string>>(cancellationToken: ct);
    if (response == null || !Version.TryParse(response.Payload, out var version))
    {
      return NullVersion;
    }

    return version;
  }

  public async Task DownloadInstallerAsync(Stream output, Version version, ProgressChanged onProgressCallback,
    CancellationToken ct = default)
  {
    using var response = await _client.GetAsync(GetStableChannelInstallerUrl(version),
      HttpCompletionOption.ResponseHeadersRead, ct);
    await using var contentStream = await response.Content.ReadAsStreamAsync(ct);

    double installerSize = response.Content.Headers.ContentLength ?? 0L;
    long totalDownloaded = 0L;
    int readBytes;
    byte[] buffer = new byte[BufferSize];
    do
    {
      readBytes = await contentStream.ReadAsync(buffer, 0, BufferSize, ct);
      totalDownloaded += readBytes;
      await output.WriteAsync(buffer, 0, readBytes, ct);

      var progress = (int)Math.Floor(totalDownloaded / installerSize * 100);
      onProgressCallback((long)installerSize, totalDownloaded, progress);
    } while (readBytes > 0);
  }

  private Uri GetStableChannelInstallerUrl(Version version) =>
    new($"/v1/updates/stable/{TargetOs}/{Arch}/{version}{InstallerExt}", UriKind.RelativeOrAbsolute);

  private static string Arch => Environment.Is64BitOperatingSystem ? "x64" : "x86";
}