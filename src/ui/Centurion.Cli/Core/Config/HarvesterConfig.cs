using System.Runtime.InteropServices;

namespace Centurion.Cli.Core.Config;

public class HarvesterConfig
{
  public HarvesterConfig()
  {
    ChromiumBaseUserDir = Path.Combine(ChromiumCachesDir, "UserData");
    ChromiumDownloadDir = Path.Combine(ChromiumCachesDir, "Browser");
    BrowserExePath = Path.Combine(ChromiumDownloadDir,
      RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "chrome.exe"
        : "Google Chrome.app/Contents/MacOS/Google Chrome");
  }

  public string ChromeDistroArchiveLocation { get; init; } = null!;

  public string ChromiumCachesDir { get; } =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "CenturionCaches");

  public string ChromiumBaseUserDir { get; }
  public string ChromiumDownloadDir { get; }
  public string BrowserExePath { get; }
}