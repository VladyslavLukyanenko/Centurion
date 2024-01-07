namespace Centurion.CloudManager.Web.Services;

public class ScopedBackgroundWorkersExecutor : BackgroundService
{
  private static readonly TimeSpan IterationsDelay = TimeSpan.FromMilliseconds(200);
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ScopedBackgroundWorkersExecutor> _logger;

  public ScopedBackgroundWorkersExecutor(IServiceProvider serviceProvider,
    ILogger<ScopedBackgroundWorkersExecutor> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var scope = _serviceProvider.CreateAsyncScope();
      var scopedWorkers = scope.ServiceProvider.GetServices<IScopedBackgroundService>();
      foreach (var worker in scopedWorkers)
      {
        try
        {
          await worker.ExecuteIteration(stoppingToken);
        }
        catch (OperationCanceledException exc) when (exc.CancellationToken.IsCancellationRequested)
        {
          break;
        }
        catch (Exception exc)
        {
          _logger.LogError(exc, "Failed to execute scoped worker {WorkerName}", worker.GetType().Name);
        }
      }

      await scope.DisposeAsync();
      await Task.Delay(IterationsDelay, stoppingToken);
    }
  }
}