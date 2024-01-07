using Centurion.Accounts.App.Audit.Data;
using Centurion.Accounts.App.Audit.Services;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Centurion.Accounts.Core.Collections;

namespace Centurion.Accounts.Audit.Controllers;

// [Authorize] admin
public class AuditController : SecuredControllerBase
{
  private readonly IChangeSetProvider _changeSetProvider;

  public AuditController(IServiceProvider provider, IChangeSetProvider changeSetProvider)
    : base(provider)
  {
    _changeSetProvider = changeSetProvider;
  }

  [HttpGet]
  [ProducesResponseType(typeof(ApiContract<IPagedList<ChangeSetData>>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetChangesPageAsync([FromQuery] ChangeSetPageRequest pageRequest,
    CancellationToken ct = default)
  {
    var page = await _changeSetProvider.GetChangeSetPageAsync(pageRequest, ct);
    return Ok(page);
  }

  [HttpGet("{entryId:guid}")]
  [ProducesResponseType(typeof(ApiContract<ChangesetEntryPayloadData>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetChangeSetEntryPayloadPageAsync(Guid entryId)
  {
    var payload = await _changeSetProvider.GetPayloadAsync(entryId);
    if (payload == null)
    {
      return NotFound();
    }

    return Ok(payload);
  }
}