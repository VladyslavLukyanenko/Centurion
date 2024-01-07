using System.Runtime.CompilerServices;
using Centurion.Contracts.Monitor.Integration;
using Centurion.TaskManager;
using Elastic.Apm.Api;
using Microsoft.Extensions.Logging;

namespace Centurion.Monitor.Domain.Services;

public class GenericWatcher : IWatcher
{
  private readonly IStoreMonitorFactory _monitorFactory;
  private readonly ILogger<GenericWatcher> _logger;

  private CancellationTokenSource? _cts;
  private readonly ITracer _tracer;

  public GenericWatcher(IStoreMonitorFactory monitorFactory, ILogger<GenericWatcher> logger, ITracer tracer)
  {
    _monitorFactory = monitorFactory;
    _logger = logger;
    _tracer = tracer;
  }

  public ValueTask DisposeAsync()
  {
    Cancel();
    Target = null;

    return default;
  }

  public async ValueTask SpawnAsync(MonitorTarget target, CancellationToken ct)
  {
    Cancel();
    Target = target;
    _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    _logger.LogDebug("Watcher {Site}.{Sku} spawned", target.Module, target.Sku);
    IStoreMonitor monitor = _monitorFactory.CreateFor(target.Module);
    while (!_cts.IsCancellationRequested)
    {
      var checkActivity = _tracer.StartAttachedTransaction("check_status", "watcher");
      checkActivity?.SetLabel("target.module", target.Module.ToString());
      checkActivity?.SetLabel("target.sku", target.Sku);
      checkActivity?.SetLabel("target.url", target.PageUrl.ToString());
      checkActivity?.SetLabel("target.correlation_id", target.TaskId.ToString());
      checkActivity?.SetLabel("user.id", target.UserId);
      try
      {
        await foreach (var change in ExecuteMonitoringIteration(target, monitor, _cts.Token))
        {
          var args = new WatcherStatusChangedEventArgs(change, target, _cts.Token);
          await StatusChanged.InvokeIfNotEmptyAsync(this, args);

          if (change.Status.IsCompleted())
          {
            return;
          }
        }
      }
      catch (Exception exc)
      {
        checkActivity?.CaptureException(exc);
        if (exc is OperationCanceledException)
        {
          _logger.LogWarning(exc, "Operation cancelled");
        }
        else
        {
          _logger.LogCritical(exc, "An error occurred on check status");
        }
      }
      finally
      {
        checkActivity?.End();
        _logger.LogDebug("Watcher {Site}.{Sku} finished execution", target.Module, target.Sku);
      }
    }
  }

  public IAsyncEnumerable<MonitoringStatusChanged> ExecuteMonitoringIteration(MonitorTarget target,
    CancellationToken ct = default)
  {
    Target = target;
    IStoreMonitor monitor = _monitorFactory.CreateFor(target.Module);
    return ExecuteMonitoringIteration(target, monitor, ct);
  }

  private async IAsyncEnumerable<MonitoringStatusChanged> ExecuteMonitoringIteration(MonitorTarget target,
    IStoreMonitor monitor, [EnumeratorCancellation] CancellationToken ct)
  {
    // yield return MonitoringStatusChanged.Monitoring(target);
    while (!ct.IsCancellationRequested && !monitor.IsInitialized)
    {
      await foreach (var change in monitor.Initialize(target, ct))
      {
        yield return change;
      }
    }

    await foreach (var change in monitor.Monitor(target, ct))
    {
      yield return change;
    }
  }

  private void Cancel()
  {
    _cts?.Cancel();
    _cts?.Dispose();
  }

  public event AsyncEventHandler<WatcherStatusChangedEventArgs>? StatusChanged;
  public MonitorTarget? Target { get; private set; }
}