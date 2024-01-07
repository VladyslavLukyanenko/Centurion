using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging.Abstractions;

namespace Centurion.CloudManager.Web.Services;

public class TokenBucketThrottlingQueue<T> : IThrottlingQueue<T>
{
  private readonly ILogger _logger;
  private readonly CompositeDisposable _disposable = new();
  private readonly Subject<T> _consumer = new();
  private readonly IObservable<IEnumerable<T>> _batcher;

  private readonly SemaphoreSlim _gates = new(1, 1);
  private readonly HashSet<T> _schedulerRegistry = new();
  private readonly BlockingCollection<object> _throttlingTokens = new();
  private readonly BlockingCollection<T> _queue = new();

  public TokenBucketThrottlingQueue(int refillRatePerSec, int maxCapacity, bool refillToMax = false,
    IScheduler? processOnScheduler = null, TimeSpan? bufferingDelay = null, ILogger? logger = null)
  {
    _logger = logger ?? NullLogger.Instance;
    _batcher = _consumer
      .Buffer(bufferingDelay.GetValueOrDefault(TimeSpan.FromMilliseconds(200)),
        processOnScheduler ?? TaskPoolScheduler.Default)
      .Where(b => b.Any())
      .Replay(1)
      .RefCount();

    var resourceToken = new object();

    if (refillToMax)
    {
      foreach (var _ in Enumerable.Range(0, maxCapacity))
      {
        _throttlingTokens.Add(resourceToken);
      }
    }

    Observable.Interval(TimeSpan.FromSeconds(1f / refillRatePerSec))
      .Where(_ => _throttlingTokens.Count < maxCapacity)
      .Subscribe(_ =>
      {
        _throttlingTokens.Add(resourceToken);
        _logger.LogDebug("Refilled bucket token");
      })
      .DisposeWith(_disposable);
  }

  public ISet<T> SubmitUniqueRange(params T[] items)
  {
    try
    {
      _gates.Wait();
      var scheduled = new HashSet<T>(items.Length);
      foreach (var item in items)
      {
        if (!_schedulerRegistry.Contains(item))
        {
          _schedulerRegistry.Add(item);
          _queue.Add(item);
          _logger.LogDebug("Submitted new item to queue {@Item}", item);
          scheduled.Add(item);
        }
      }

      return scheduled;
    }
    finally
    {
      _gates.Release();
    }
  }

  public void CompletedProcessing(T item)
  {
    try
    {
      _gates.Wait(CancellationToken.None);
      _schedulerRegistry.Remove(item);
    }
    finally
    {
      _gates.Release();
    }
  }

  public void ProcessBlocking(CancellationToken stoppingToken)
  {
    foreach (var _ in _throttlingTokens.GetConsumingEnumerable(stoppingToken))
    {
      var next = _queue.Take(stoppingToken);

      _logger.LogDebug("Pushing next item");
      _consumer.OnNext(next);
    }
  }

  public IDisposable Subscribe(IObserver<IEnumerable<T>> observer)
  {
    var sub = _batcher.Subscribe(observer);
    _disposable.Add(sub);
    return sub;
  }

  public void Dispose()
  {
    _disposable.Dispose();
    _consumer.Dispose();
    _gates.Dispose();
    _throttlingTokens.Dispose();
    _queue.Dispose();
  }
}