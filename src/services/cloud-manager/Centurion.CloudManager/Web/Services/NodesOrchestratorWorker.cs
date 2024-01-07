using System.Diagnostics;
using System.Runtime.CompilerServices;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;

namespace Centurion.CloudManager.Web.Services;

public class NodesOrchestratorWorker : IScopedBackgroundService
{
  private static readonly TimeSpan ExpectedMaxExecutionTime = TimeSpan.FromMilliseconds(1_000);

  private readonly INodeSnapshotRepository _snapshotRepository;
  private readonly IEnumerable<IInfrastructureClient> _infrastructureClients;
  private readonly IExecutionScheduler _scheduler;
  private readonly INodeLifetimeManager _lifetimeManager;
  private readonly ILogger<NodesOrchestratorWorker> _logger;

  public NodesOrchestratorWorker(INodeSnapshotRepository snapshotRepository,
    IEnumerable<IInfrastructureClient> infrastructureClients, IExecutionScheduler scheduler,
    INodeLifetimeManager lifetimeManager, ILogger<NodesOrchestratorWorker> logger)
  {
    _snapshotRepository = snapshotRepository;
    _infrastructureClients = infrastructureClients;
    _scheduler = scheduler;
    _lifetimeManager = lifetimeManager;
    _logger = logger;
  }

  public async ValueTask ExecuteIteration(CancellationToken ct)
  {
    using var __ = new PerfMeasure(_logger, ExpectedMaxExecutionTime);
    _logger.LogDebug("Executing orchestrator loop iteration");
    var pending = await _snapshotRepository.GetAllPending(ct);
    var nodes = (await Task.WhenAll(_infrastructureClients.Select(c => c.RefreshNodesAsync(ct))))
      .SelectMany(n => n)
      .ToArray();

    foreach (var node in nodes.Where(n => n.User is not null && !n.IsAlive))
    {
      node.Unbind();
      _scheduler.ScheduleShutdown(node.Id);
    }

    if (pending.Count != 0)
    {
      _scheduler.ScheduleStartNewNodes(pending.Select(_ => _.User.Id).ToArray());
    }

    _lifetimeManager.Update(nodes.Where(_ => _.Status is NodeStatus.Running));
  }

  private readonly struct PerfMeasure : IDisposable
  {
    private readonly ILogger _logger;
    private readonly Stopwatch _timer;
    private readonly TimeSpan _max;
    private readonly string? _caller;

    public PerfMeasure(ILogger logger, TimeSpan max, [CallerMemberName] string? caller = null)
    {
      _logger = logger;
      _timer = Stopwatch.StartNew();
      _max = max;
      _caller = caller;
    }

    public void Dispose()
    {
      _timer.Stop();
      if (_timer.Elapsed > _max)
      {
        _logger.LogWarning("Execution of {Method} took {Elapsed} but expected to not exceed {MaxExpected}", _caller,
          _timer.Elapsed, _max);
      }
    }
  }
}