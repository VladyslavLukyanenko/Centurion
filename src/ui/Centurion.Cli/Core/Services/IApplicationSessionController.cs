namespace Centurion.Cli.Core.Services;

public interface IApplicationSessionController
{
  ValueTask StartSession(CancellationToken ct = default);
  ValueTask StopSession(CancellationToken ct = default);
}