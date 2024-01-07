using Centurion.Contracts.TaskManager;
using NodaTime;

namespace Centurion.TaskManager.Application.Services;

public interface IPresetProvider
{
  ValueTask<IList<PresetData>> GetList(Instant startAt, Instant endAt, CancellationToken ct = default);
}