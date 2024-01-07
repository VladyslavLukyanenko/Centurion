using Centurion.SeedWork.Events;

namespace Centurion.SeedWork.Primitives;

public interface IEventSource
{
  IReadOnlyList<IDomainEvent> DomainEvents { get; }
  void AddDomainEvent(IDomainEvent eventItem);
  void RemoveDomainEvent(IDomainEvent eventItem);
  void ClearDomainEvents();
}