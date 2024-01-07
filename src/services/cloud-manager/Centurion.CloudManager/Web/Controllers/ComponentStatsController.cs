using Centurion.CloudManager.App.Model;
using Centurion.CloudManager.App.Services;
using Centurion.SeedWork.Web.Foundation.Model;
using Centurion.TaskManager.Web.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.CloudManager.Web.Controllers;

public class ComponentStatsController : SecuredControllerBase
{
  private readonly IComponentsStateRegistry _registry;

  public ComponentStatsController(IComponentsStateRegistry registry, IServiceProvider provider)
    : base(provider)
  {
    _registry = registry;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ApiContract<Dictionary<string, ComponentStatsEntry[]>>), StatusCodes.Status200OK)]
  public IActionResult GetStats()
  {
    var entries = _registry.GetGroupedEntries(TimeSpan.FromMinutes(1))
      .ToDictionary(_ => _.Key, _ => _.ToArray());
    return Ok(entries);
  }
}