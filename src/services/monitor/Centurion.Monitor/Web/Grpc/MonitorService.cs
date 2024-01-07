using Centurion.Contracts.Monitor;
using Centurion.Contracts.Monitor.Integration;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;
using Centurion.TaskManager;
using Elastic.Apm.Api;
using Grpc.Core;
using static Centurion.Contracts.Monitor.Monitor;

namespace Centurion.Monitor.Web.Grpc;

public class MonitorService : MonitorBase
{
  private readonly IWatcherFactory _watcherFactory;
  private readonly ILogger<MonitorService> _logger;
  private readonly ITracer _tracer;

  public MonitorService(IWatcherFactory watcherFactory, ILogger<MonitorService> logger, ITracer tracer)
  {
    _watcherFactory = watcherFactory;
    _logger = logger;
    _tracer = tracer;
  }

  public override async Task WatchStreamed(WatchCommand request,
    IServerStreamWriter<MonitoringStatusChanged> responseStream, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var monitorTarget = MapMonitorTarget(request);

    _logger.LogDebug("Spawning watcher for target '{Site}.{Target}'", monitorTarget.Module, monitorTarget.Sku);

    var spawnActivity = _tracer.StartAttachedTransaction("spawn_watcher", TraceConsts.Activities.Watcher);
    spawnActivity?.SetLabel("url", monitorTarget.PageUrl.ToString());

    var watcher = _watcherFactory.Create(monitorTarget);

    try
    {
      await foreach (var status in watcher.ExecuteMonitoringIteration(monitorTarget, ct))
      {
        await responseStream.WriteAsync(status);
      }

      _logger.LogDebug("Watcher for target '{Site}.{Target}' gracefully completed execution", monitorTarget.Module,
        monitorTarget.Sku);
    }
    catch (Exception exc)
    {
      spawnActivity?.CaptureException(exc);
      _logger.LogCritical(exc, "Error on spawn watcher");
      throw;
    }
    finally
    {
      spawnActivity?.End();
    }
  }

  private static MonitorTarget MapMonitorTarget(WatchCommand request)
  {
    if (string.IsNullOrWhiteSpace(request.TaskId))
    {
      throw new RpcException(new Status(StatusCode.FailedPrecondition, "No correlationId"));
    }

    var proxies = request.ProxyPool?.Proxies.Select(p => p.ToUri()).ToArray() ?? Array.Empty<Uri>();

    MonitorTarget t = new()
    {
      TaskId = Guid.Parse(request.TaskId),
      Module = request.Module,
      Sku = request.Product.Sku,
      Picture = request.Product.Image,
      Title = request.Product.Name,
      PageUrl = new Uri(request.Product.Link),
      UserId = request.UserId,
      Extra = request.Extra,
      ModuleConfig = request.ModuleConfig.ToByteArray(),
      Settings = new MonitorSettings
      {
        Proxies = proxies,
      }
    };

    return t;
  }
}