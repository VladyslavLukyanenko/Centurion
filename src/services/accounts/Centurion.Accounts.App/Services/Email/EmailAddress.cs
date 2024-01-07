namespace Centurion.Accounts.App.Services.Email;

public class EmailAddress
{
  public EmailAddress(string email, string name)
  {
    Email = email;
    Name = name;
  }

  public EmailAddress(string email)
    : this(email, string.Empty)
  {
  }

  public string Email { get; }

  public string Name { get; }
}