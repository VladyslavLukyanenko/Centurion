namespace Centurion.Accounts.App.Identity.Model;

public class UserData
{
  public long Id { get; set; }
  public string Email { get; set; } = null!;
  public bool IsEmailConfirmed { get; set; }

  public bool IsLockedOut { get; set; }

  public string Name { get; set; } = null!;

  public IList<string> Roles { get; set; } = new List<string>();
}