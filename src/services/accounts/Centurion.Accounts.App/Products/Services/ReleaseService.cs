using AutoMapper;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.App.Products.Services;

public class ReleaseService : IReleaseService
{
  private readonly IReleaseRepository _releaseRepository;
  private readonly IMapper _mapper;

  public ReleaseService(IReleaseRepository releaseRepository, IMapper mapper)
  {
    _releaseRepository = releaseRepository;
    _mapper = mapper;
  }

  public async ValueTask<long> CreateAsync(Guid dashboardId, SaveReleaseCommand cmd, CancellationToken ct = default)
  {
    Release release = new(cmd.Password, cmd.InitialStock, cmd.Title, cmd.Type, cmd.PlanId, dashboardId, cmd.IsActive);
    release = await _releaseRepository.CreateAsync(release, ct);
    return release.Id;
  }

  public ValueTask UpdateAsync(Release release, SaveReleaseCommand cmd, CancellationToken ct = default)
  {
    _mapper.Map(cmd, release);
    _releaseRepository.Update(release);

    return ValueTask.CompletedTask;
  }
}