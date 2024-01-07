namespace Centurion.Accounts.Core.Events;

public class IdentityRemoved
  : DomainEvent
{
  public IdentityRemoved(long id)
  {
    Id = id;
  }

  public long Id { get; }
}