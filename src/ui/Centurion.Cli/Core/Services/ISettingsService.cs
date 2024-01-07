namespace Centurion.Cli.Core.Services;

public interface ISettingsService
{
  ValueTask<T?> ReadSettingsOrDefaultAsync<T>(string name, Func<T>? defaultFactory = null,
    CancellationToken ct = default);

  ValueTask WriteSettingsAsync<T>(string name, T settings, CancellationToken ct = default);
}