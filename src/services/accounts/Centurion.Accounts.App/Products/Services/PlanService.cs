using AutoMapper;
using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;

namespace Centurion.Accounts.App.Products.Services;

public class PlanService : IPlanService
{
  private readonly IPlanRepository _planRepository;
  private readonly IMapper _mapper;

  public PlanService(IPlanRepository planRepository, IMapper mapper)
  {
    _planRepository = planRepository;
    _mapper = mapper;
  }
    
  public async ValueTask<long> CreateAsync(Guid dashboardId, PlanData data, CancellationToken ct = default)
  {
    var entity = new Plan(dashboardId);
    _mapper.Map(data, entity);
    var created = await _planRepository.CreateAsync(entity, ct);
    return created.Id;
  }

  public ValueTask UpdateAsync(Plan plan, PlanData data, CancellationToken ct = default)
  {
    _mapper.Map(data, plan);
    _planRepository.Update(plan);

    return default;
  }
}