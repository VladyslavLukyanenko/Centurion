using Centurion.Accounts.App.Identity.Model;

namespace Centurion.Accounts.App.Commands;

public class RegisterWithEmailCommand : UserData
{
  public string Password { get; set; } = null!;
}