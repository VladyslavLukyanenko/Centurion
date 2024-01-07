using Centurion.Cli.Core.ViewModels.Home;
using DynamicData;
using NodaTime;

namespace Centurion.Cli.Core.Services;

public interface IPresetsService : IAppStateHolder
{
  IObservableCache<PresetItemViewModel, LocalDate> Presets { get; }
  ValueTask Fetch(Instant startAt, Instant endAt, CancellationToken ct = default);
}
