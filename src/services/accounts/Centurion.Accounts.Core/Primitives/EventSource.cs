using Centurion.Accounts.Core.Events;

namespace Centurion.Accounts.Core.Primitives;

public abstract class EventSource
{
  private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

  public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

  public void AddDomainEvent(IDomainEvent eventItem)
  {
    _domainEvents.Add(eventItem);
  }

  public void RemoveDomainEvent(IDomainEvent eventItem)
  {
    _domainEvents.Remove(eventItem);
  }

  public void ClearDomainEvents()
  {
    _domainEvents.Clear();
  }
}