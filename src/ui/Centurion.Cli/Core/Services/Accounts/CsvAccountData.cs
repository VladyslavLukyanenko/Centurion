using System.Reflection;

#nullable disable
namespace Centurion.Cli.Core.Services.Accounts;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class CsvAccountData
{
  public string GroupName { get; set; }
  public string Email { get; set; }
  public string Password { get; set; }
  public string AccessToken { get; set; }
}