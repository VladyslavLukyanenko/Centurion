using Centurion.Cli.Core.Domain;

namespace Centurion.Cli.Core.Services;

public interface IGeneralSettingsService : IAppStateHolder
{
  IObservable<GeneralSettings> Settings { get; }
  ValueTask Save(GeneralSettings settings);
  ValueTask UpdateCheckoutSound(string filePath, CancellationToken ct = default);
  ValueTask UpdateDeclineSound(string filePath, CancellationToken ct = default);
  ValueTask PlayCheckoutSound(CancellationToken ct = default);
  ValueTask PlayDeclineSound(CancellationToken ct = default);
  void ResetCheckoutSound();
  void ResetDeclineSound();
}