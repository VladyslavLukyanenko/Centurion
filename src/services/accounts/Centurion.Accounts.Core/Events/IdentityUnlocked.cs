namespace Centurion.Accounts.Core.Events;

public class IdentityUnlocked : DomainEvent
{
  public IdentityUnlocked(long id)
  {
    Id = id;
  }

  public long Id { get; }
}