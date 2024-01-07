using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Foundation.Model;
using Centurion.Accounts.Foundation.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Products.Controllers;

public class UpdatesController : SecuredDashboardBoundControllerBase
{
  private readonly IArtifactsFileProvider _artifactsFileProvider;

  public UpdatesController(IServiceProvider provider, IArtifactsFileProvider artifactsFileProvider)
    : base(provider)
  {
    _artifactsFileProvider = artifactsFileProvider;
  }

  [HttpGet("{channel}/{os}/{arch}")]
  [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiContract<Version>))]
  public IActionResult GetLatestVersion(string channel, string os, string arch)
  {
    var version = _artifactsFileProvider.GetLatestVersion(CurrentDashboardId, channel, os, arch);
    if (version.HasNoValue)
    {
      return NotFound();
    }

    return Ok(version.Value!);
  }

  [HttpGet("{channel}/{os}/{arch}/{version}.{ext}")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public IActionResult DownloadArtifact(string channel, string os, Version version, string arch, string ext)
  {
    var str = _artifactsFileProvider.TryOpenStreamOfVersion(CurrentDashboardId, channel, os, arch, version, ext);
    if (str is null)
    {
      return NotFound();
    }

    return File(str, "application/octet-stream");
  }
}