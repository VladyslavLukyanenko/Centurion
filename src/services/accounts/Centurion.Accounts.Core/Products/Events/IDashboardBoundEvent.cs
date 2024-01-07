using Centurion.Accounts.Core.Events;

namespace Centurion.Accounts.Core.Products.Events;

public interface IDashboardBoundEvent : IIntegrationEvent
{
  Guid DashboardId { get; }
}