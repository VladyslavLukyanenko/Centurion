using Centurion.Accounts.App.Identity.Model;

namespace Centurion.Accounts.App.Commands;

public class CreateWithConfirmedEmailCommand : UserData
{
  public string[] RoleNames { get; set; } = Array.Empty<string>();
  public string Password { get; set; } = null!;
}