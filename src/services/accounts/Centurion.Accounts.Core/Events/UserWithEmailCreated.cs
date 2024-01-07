using Centurion.Accounts.Core.Identity;

namespace Centurion.Accounts.Core.Events;

public class UserWithEmailCreated : DomainEvent
{
  public UserWithEmailCreated(User user)
  {
    User = user;
  }

  public User User { get; }
}