using Centurion.Cli.Core.Services.Modules;
using Centurion.Cli.Core.ViewModels.Home;
using Centurion.Contracts.TaskManager;
using CSharpFunctionalExtensions;
using DynamicData;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using NodaTime.Extensions;
using Duration = NodaTime.Duration;

namespace Centurion.Cli.Core.Services;

public class PresetsService : ExecutionStatusProviderBase, IPresetsService
{
  private readonly SourceCache<PresetItemViewModel, LocalDate> _presets = new(_ => _.Date);
  private readonly Presets.PresetsClient _presetsClient;
  private const int MaxDaysToFetch = 15;

  public PresetsService(Presets.PresetsClient presetsClient)
  {
    _presetsClient = presetsClient;
    Presets = _presets.AsObservableCache();
  }

  public IObservableCache<PresetItemViewModel, LocalDate> Presets { get; }

  public async ValueTask Fetch(Instant startAt, Instant endAt, CancellationToken ct = default)
  {
    if ((endAt - startAt).TotalDays > MaxDaysToFetch)
    {
      endAt = startAt + Duration.FromDays(MaxDaysToFetch);
    }

    var presetList = await _presetsClient.GetListAsync(new PresetsRequest
      {
        StartAt = Timestamp.FromDateTimeOffset(startAt.ToDateTimeOffset()),
        EndAt = Timestamp.FromDateTimeOffset(endAt.ToDateTimeOffset())
      })
      .TrackProgress(FetchingTracker);

    var loadedPresets = presetList.Presets.ToLookup(_ => _.ExpectedAt.ToDateTime().ToLocalDateTime().Date);
    DateTimeZone tz = DateTimeZoneProviders.Bcl.GetSystemDefault();
    var presets = new List<PresetItemViewModel>(MaxDaysToFetch);
    var endDate = endAt.InZone(tz).Date;
    var startDate = startAt.InZone(tz).Date;
    for (var date = startDate; date <= endDate; date = date.PlusDays(1))
    {
      presets.Add(new PresetItemViewModel
      {
        ShouldShowMonthName = date != startDate && date.Month != date.PlusDays(-1).Month,
        Date = date,
        Presets =
        {
          loadedPresets[date]
        }
      });
    }

    _presets.Clear();
    _presets.AddOrUpdate(presets);
  }

  public async ValueTask<Result> InitializeAsync(CancellationToken ct = default)
  {
    var now = SystemClock.Instance.GetCurrentInstant();
    await Fetch(now, now + Duration.FromDays(MaxDaysToFetch), ct);
    return Result.Success();
  }

  public void ResetCache()
  {
    _presets.Clear();
  }
}