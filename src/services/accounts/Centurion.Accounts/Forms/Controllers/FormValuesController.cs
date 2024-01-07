using Centurion.Accounts.App.Forms.Services;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Core.Forms;
using Centurion.Accounts.Core.Forms.Services;
using Centurion.Accounts.Foundation.Authorization;

namespace Centurion.Accounts.Forms.Controllers;

public class FormValuesController : SecuredDashboardBoundControllerBase
{
  private readonly IFormValueService _formValueService;
  private readonly IFormRepository _formRepository;

  public FormValuesController(IServiceProvider provider, IFormValueService formValueService,
    IFormRepository formRepository) : base(provider)
  {
    _formValueService = formValueService;
    _formRepository = formRepository;
  }

  [HttpPost("{formId:long}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async ValueTask<IActionResult> SubmitAsync([FromBody] IList<FormFieldValue> fields, long formId,
    CancellationToken ct)
  {
    Form? form = await _formRepository.GetByIdAsync(formId, ct);
    if (form == null)
    {
      return NotFound();
    }

    await AppAuthorizationService.AdminOrMemberAsync(form.DashboardId)
      .OrThrowForbid();

    var submitResult = await _formValueService.SubmitAsync(form, fields, CurrentUserId,ct);
    if (submitResult.IsFailure)
    {
      return BadRequest();
    }

    return NoContent();
  }
}