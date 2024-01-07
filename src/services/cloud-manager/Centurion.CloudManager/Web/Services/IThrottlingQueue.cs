namespace Centurion.CloudManager.Web.Services;

public interface IThrottlingQueue<T> : IObservable<IEnumerable<T>>, IDisposable
{
  ISet<T> SubmitUniqueRange(params T[] items);
  void CompletedProcessing(T item);
  void ProcessBlocking(CancellationToken stoppingToken);
}