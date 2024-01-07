using Centurion.Contracts.CloudManager;
using Centurion.TaskManager.Core.Services;
using Grpc.Core;

namespace Centurion.TaskManager.Web.Services;

public class CloudManagerWorker : BackgroundService
{
  private readonly ICloudManager _manager;
  private readonly ILogger<CloudManagerWorker> _logger;
  private readonly IServiceProvider _serviceProvider;

  public CloudManagerWorker(ICloudManager manager, ILogger<CloudManagerWorker> logger, IServiceProvider serviceProvider)
  {
    _manager = manager;
    _logger = logger;
    _serviceProvider = serviceProvider;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<Cloud.CloudClient>();
        await _manager.EstablishConnection(client, stoppingToken);
      }
      catch (Exception exc)
      {
        if (exc is RpcException r)
        {
          _logger.LogError("Cloud connection issues: {Details}, {Code}", r.Status.Detail, r.StatusCode);
        }
        else
        {
          _logger.LogError(exc, "Connection with cloud closed");
        }
        await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
      }
    }
  }
}