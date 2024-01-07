using Centurion.Accounts.App.Products.Model;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Authorization;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Products.Controllers;

public class PlansController : SecuredDashboardBoundControllerBase
{
  private readonly IPlanProvider _planProvider;
  private readonly IPlanRepository _planRepository;
  private readonly IPlanService _planService;

  public PlansController(IServiceProvider provider, IPlanProvider planProvider, IPlanRepository planRepository,
    IPlanService planService)
    : base(provider)
  {
    _planProvider = planProvider;
    _planRepository = planRepository;
    _planService = planService;
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<PlanData[]>))]
  public async ValueTask<IActionResult> GetAllAsync(CancellationToken ct)
  {
    var list = await _planProvider.GetAllAsync(CurrentDashboardId, ct);
    return Ok(list);
  }

  [HttpDelete("{id:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.PlansDelete)]
  public async ValueTask<IActionResult> RemoveAsync(long id, CancellationToken ct)
  {
    Plan? plan = await _planRepository.GetByIdAsync(id, ct);
    if (plan == null)
    {
      return NotFound();
    }

    _planRepository.Remove(plan);
    return NoContent();
  }


  [HttpPost]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.PlansManage)]
  public async ValueTask CreateAsync([FromBody] PlanData data, CancellationToken ct)
  {
    await _planService.CreateAsync(CurrentDashboardId, data, ct);
  }

  [HttpPut("{id:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [AuthorizePermission(Permissions.PlansManage)]
  public async ValueTask<IActionResult> UpdateAsync(long id, [FromBody] PlanData data, CancellationToken ct)
  {
    var plan = await _planRepository.GetByIdAsync(id, ct);
    if (plan == null)
    {
      return NotFound();
    }

    await _planService.UpdateAsync(plan, data, ct);
    return NoContent();
  }
}