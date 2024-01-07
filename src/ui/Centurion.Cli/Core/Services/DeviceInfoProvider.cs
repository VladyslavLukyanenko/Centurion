using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Centurion.Cli.Core.Services;

public class DeviceInfoProvider : IDeviceInfoProvider
{
  private static readonly Lazy<Task<string>> Hwid;

  static DeviceInfoProvider()
  {
    Hwid = new Lazy<Task<string>>(GenerateHwid, LazyThreadSafetyMode.ExecutionAndPublication);
  }

  public async ValueTask<string> GetHwidAsync(CancellationToken ct) => await Hwid.Value;

  private static Task<string> GenerateHwid() => Task.Run(() =>
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      string hwid = CreateMD5(WindowsHWID.Value());
      return Task.FromResult(hwid);
    }

    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
      string hwid = CreateMD5(Bash("system_profiler SPHardwareDataType | awk '/UUID/ { print $3; }'"));
      return Task.FromResult(hwid);
    }

    return Task.FromResult("<FAKE>");
    // throw new PlatformNotSupportedException();
  });

  private static string CreateMD5(string input)
  {
    using (MD5 md5 = MD5.Create())
    {
      byte[] inputBytes = Encoding.ASCII.GetBytes(input);
      byte[] hashBytes = md5.ComputeHash(inputBytes);

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < hashBytes.Length; i++)
      {
        sb.Append(hashBytes[i].ToString("X2"));
      }

      return sb.ToString();
    }
  }

  private static string Bash(string cmd)
  {
    var escapedArgs = cmd.Replace("\"", "\\\"");

    var process = new Process()
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
    string result = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return result.Trim();
  }
}