namespace Centurion.CloudManager.Web.Services;

public interface IScopedBackgroundService
{
  ValueTask ExecuteIteration(CancellationToken ct);
}