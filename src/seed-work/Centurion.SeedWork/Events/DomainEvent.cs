using Centurion.Contracts.Integration;
using Google.Protobuf.WellKnownTypes;

namespace Centurion.SeedWork.Events;

public abstract class DomainEvent : IDomainEvent
{
  public EventMetadata Meta { get; } = new()
  {
    Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow)
  };
}