using MediatR;

namespace Centurion.SeedWork.Events;

public interface IDomainEvent : IIntegrationEvent, INotification
{
}