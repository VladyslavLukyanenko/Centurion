using Centurion.Accounts.Foundation.Model;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Controllers;

public class TimeZonesController : Foundation.Mvc.Controllers.ControllerBase
{
  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<TimeZoneData[]>))]
  public IActionResult GetSupportedTimeZones()
  {
    var timeZones = TimeZoneInfo.GetSystemTimeZones()
      .OrderBy(_ => _.BaseUtcOffset)
      .ThenBy(_ => _.DisplayName)
      .Select(_ => new TimeZoneData
      {
        Id = _.Id,
        Name = _.DisplayName
      });
    return Ok(timeZones);
  }
}

public class TimeZoneData
{
  public string Id { get; set; } = null!;
  public string Name { get; set; } = null!;
}