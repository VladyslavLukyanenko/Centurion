using Centurion.Contracts.Integration;
using Centurion.SeedWork.Events;

// ReSharper disable once CheckNamespace
namespace Centurion.Contracts.Monitor.Integration;

public partial class MonitoringStatusChanged : IIntegrationEvent
{
  public MonitoringStatusChanged(Guid correlationId, string sku, Module module, string userId, TaskStatusData status)
  {
    Status = status;
    Sku = sku;
    Module = module;
    Meta = new EventMetadata
    {
      TaskId = correlationId.ToString(),
      UserId = userId
    };
  }
}