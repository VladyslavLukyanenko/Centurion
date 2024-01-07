using Centurion.Contracts.TaskManager;
using Centurion.TaskManager.Application.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using NodaTime.Extensions;

namespace Centurion.TaskManager.Web.Grpc;

[Authorize]
public class PresetsService : Presets.PresetsBase
{
  private readonly IPresetProvider _presetProvider;

  public PresetsService(IPresetProvider presetProvider)
  {
    _presetProvider = presetProvider;
  }

  public override async Task<PresetList> GetList(PresetsRequest request, ServerCallContext context)
  {
    var list = await _presetProvider.GetList(request.StartAt.ToDateTimeOffset().ToInstant(),
      request.EndAt.ToDateTimeOffset().ToInstant(), context.CancellationToken);

    return new PresetList
    {
      Presets = { list }
    };
  }
}