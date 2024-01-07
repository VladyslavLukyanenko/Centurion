using System.Diagnostics;

namespace Centurion.Cli.Core;

internal class PlatformInteropUtils
{
  public static void ShowNativeMacOSAlert(string title, string message)
  {
    var cmd =
      $"osascript -e 'set theAlertText to \"{title}\"' -e 'set theAlertMessage to \"{message}\"' -e 'display alert theAlertText message theAlertMessage as critical buttons {{\"Ok\"}} default button \"Ok\"'";
    Bash(cmd);
  }

  public static void Bash(string cmd)
  {
    var escapedArgs = cmd.Replace("\"", "\\\"");

    var process = new Process
    {
      StartInfo = new ProcessStartInfo
      {
        FileName = "/bin/bash",
        Arguments = $"-c \"{escapedArgs}\"",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
      }
    };
    process.Start();
    process.WaitForExit();
    _ = process.StandardOutput.ReadToEnd();
    // return result.Trim();
  }
}