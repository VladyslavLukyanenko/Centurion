using Centurion.Accounts.App.Forms.Model;
using Centurion.Accounts.App.Forms.Services;
using Centurion.Accounts.Core.Forms;
using Centurion.Accounts.Core.Forms.Services;
using Centurion.Accounts.Foundation.Authorization;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Forms.Controllers;

public class FormsController : SecuredDashboardBoundControllerBase
{
  private readonly IFormRepository _formRepository;
  private readonly IFormProvider _formProvider;

  public FormsController(IServiceProvider provider, IFormRepository formRepository, IFormProvider formProvider)
    : base(provider)
  {
    _formRepository = formRepository;
    _formProvider = formProvider;
  }

  [HttpGet("{formId:long}")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<FormData>))]
  public async ValueTask<IActionResult> GetAsync(long formId, CancellationToken ct)
  {
    FormData? data = await _formProvider.GetUserFormAsync(formId, CurrentUserId, ct);
    if (data == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrMemberAsync(data.DashboardId)
      .OrThrowForbid();

    return Ok(data);
  }

  [HttpDelete("{formId:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> RemoveAsync(long formId, CancellationToken ct)
  {
    Form? form = await _formRepository.GetByIdAsync(formId, ct);
    if (form == null)
    {
      return NotFound();
    }

    _formRepository.Remove(form);
    return NoContent();
  }
}