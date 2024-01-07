using Centurion.Accounts.App.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Controllers;

public class CountriesController : SecuredControllerBase
{
  public CountriesController(IServiceProvider provider) : base(provider)
  {
  }

  [AllowAnonymous]
  [HttpGet]
  [ProducesResponseType(typeof(List<Country>), StatusCodes.Status200OK)]
  public IActionResult GetListAsync()
  {
    return File("~/countries.json", "application/json");
  }
}