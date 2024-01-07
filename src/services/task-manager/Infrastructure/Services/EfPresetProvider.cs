using AutoMapper;
using Centurion.Contracts.TaskManager;
using Centurion.TaskManager.Application.Services;
using Centurion.TaskManager.Core.Presets;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Centurion.TaskManager.Infrastructure.Services;

public class EfPresetProvider : IPresetProvider
{
  private readonly IMapper _mapper;
  private readonly DbContext _context;

  public EfPresetProvider(IMapper mapper, DbContext context)
  {
    _mapper = mapper;
    _context = context;
  }

  public async ValueTask<IList<PresetData>> GetList(Instant startAt, Instant endAt, CancellationToken ct = default)
  {
    var list = await _context.Set<Preset>()
      .AsNoTracking()
      .Where(_ => _.ExpectedAt >= startAt && _.ExpectedAt <= endAt)
      .OrderBy(_ => _.ExpectedAt)
      .ToArrayAsync(ct);

    return _mapper.Map<List<PresetData>>(list);
  }
}