namespace Centurion.Cli.Core.Services;

public interface IAppBackgroundWorker
{
  // todo: add CancellationToken to stop services gracefully
  void Spawn();
}