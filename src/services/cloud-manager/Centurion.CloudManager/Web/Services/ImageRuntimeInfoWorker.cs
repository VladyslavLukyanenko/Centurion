using Centurion.CloudManager.Domain.Services;

namespace Centurion.CloudManager.Web.Services;

public class ImageRuntimeInfoWorker : BackgroundService
{
  private static readonly TimeSpan ImageStateRefreshDelay = TimeSpan.FromMilliseconds(200);
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ImageRuntimeInfoWorker> _logger;

  public ImageRuntimeInfoWorker(IServiceProvider serviceProvider, ILogger<ImageRuntimeInfoWorker> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      using var scope = _serviceProvider.CreateScope();
      try
      {
        var images = scope.ServiceProvider.GetRequiredService<IImagesRuntimeInfoService>();
        var client = scope.ServiceProvider.GetRequiredService<IInfrastructureClient>();

        await images.RefreshStateAsync(client.AliveNodes, stoppingToken);
      }
      catch (OperationCanceledException opCancExc)
      {
        _logger.LogError("ImageInfo fetching: " + opCancExc.Message);
      }
      catch (HttpRequestException httpExc)
      {
        _logger.LogError("ImageInfo fetching: " + httpExc.Message);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Error on image info update");
      }

      await Task.Delay(ImageStateRefreshDelay, stoppingToken);
    }
  }
}