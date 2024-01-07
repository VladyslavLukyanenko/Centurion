using Centurion.Accounts.Core.Events;

namespace Centurion.Accounts.Core.Primitives;

public interface IEventSource
{
  IReadOnlyList<IDomainEvent> DomainEvents { get; }
  void AddDomainEvent(IDomainEvent eventItem);
  void RemoveDomainEvent(IDomainEvent eventItem);
  void ClearDomainEvents();
}