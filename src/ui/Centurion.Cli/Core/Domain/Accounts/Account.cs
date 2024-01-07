using Centurion.SeedWork.Primitives;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Domain.Accounts;

public class Account : Entity
{
  [JsonConstructor]
  public Account(Guid id, string email, string password, string? accessToken = null)
    : base(id)
  {
    Email = email;
    Password = password;
    AccessToken = accessToken;
  }

  public Account(string email, string password, string? accessToken = null)
    : this(Guid.NewGuid(), email, password, accessToken)
  {
  }

  public string Email { get; private set; }
  public string Password { get; private set; }
  public string? AccessToken { get; set; }
}