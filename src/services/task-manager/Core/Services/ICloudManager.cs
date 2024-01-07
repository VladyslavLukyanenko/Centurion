using Centurion.Contracts.CloudManager;

namespace Centurion.TaskManager.Core.Services;

public interface ICloudManager
{
  Task EstablishConnection(Cloud.CloudClient client, CancellationToken stoppingToken);
}