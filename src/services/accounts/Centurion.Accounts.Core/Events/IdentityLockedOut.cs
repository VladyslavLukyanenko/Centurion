namespace Centurion.Accounts.Core.Events;

public class IdentityLockedOut : DomainEvent
{
  public IdentityLockedOut(long id, DateTimeOffset lockoutEnd)
  {
    Id = id;
    LockoutEnd = lockoutEnd;
  }

  public long Id { get; }
  public DateTimeOffset LockoutEnd { get; }
}