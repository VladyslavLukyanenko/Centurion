using NodaTime;

namespace Centurion.Accounts.Core.Events;

public abstract class DomainEvent : IDomainEvent
{
  public Instant Timestamp { get; } = SystemClock.Instance.GetCurrentInstant();
}