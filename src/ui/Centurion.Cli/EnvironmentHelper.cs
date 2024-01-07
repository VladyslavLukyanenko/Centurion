using System.Runtime.InteropServices;

namespace Centurion.Cli;

public static class EnvironmentHelper
{
  public static string GetLocalAppDataPath()
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return Environment.GetEnvironmentVariable("LOCALAPPDATA")!;
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
      return Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
             Path.Combine(Environment.GetEnvironmentVariable("HOME")!, ".local", "share");
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      return Path.Combine(Environment.GetEnvironmentVariable("HOME")!, "Library", "Application Support");
    }

    throw new NotSupportedException("Unknown OS Platform");
  }
}