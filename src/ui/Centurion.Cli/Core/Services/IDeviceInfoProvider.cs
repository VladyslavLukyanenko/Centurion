namespace Centurion.Cli.Core.Services;

public interface IDeviceInfoProvider
{
  ValueTask<string> GetHwidAsync(CancellationToken ct);
}