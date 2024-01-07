using System.Reflection;

namespace Centurion.Cli.Core.Services.Proxies;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class CsvProxyData
{
  public string GroupName { get; set; } = null!;

  public string? Password { get; set; }
  public string Username { get; set; } = null!;
  public string Url { get; set; } = null!;
}