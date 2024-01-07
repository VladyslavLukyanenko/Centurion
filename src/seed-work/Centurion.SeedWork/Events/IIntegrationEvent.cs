using Centurion.Contracts.Integration;

namespace Centurion.SeedWork.Events;

public interface IIntegrationEvent
{
  EventMetadata Meta { get; }
}