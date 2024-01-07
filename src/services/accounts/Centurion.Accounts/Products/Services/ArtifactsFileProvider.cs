using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Products.Services;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.FileProviders;

namespace Centurion.Accounts.Products.Services;

public class ArtifactsFileProvider : PhysicalFileProvider, IArtifactsFileProvider
{
  public ArtifactsFileProvider(ArtifactsConfig cfg) : base(cfg.BasePath)
  {
  }

  public Maybe<Version?> GetLatestVersion(Guid dashboardId, string channel, string os, string arch)
  {
    var contents = GetDirectoryContents(Path.Combine(dashboardId.ToString(), channel, os, arch));
    return contents
      .Select(_ => Path.GetFileNameWithoutExtension(_.Name))
      .Select(f => Version.TryParse(f, out var v) ? v : null)
      .Where(v => v != null)
      .OrderByDescending(_ => _)
      .FirstOrDefault();
  }

  public Stream? TryOpenStreamOfVersion(Guid dashboardId, string channel, string os, string arch, Version version, string ext)
  {
    var file = GetFileInfo(Path.Combine(dashboardId.ToString(), channel, os, arch, version + "." + ext));
    if (!file.Exists)
    {
      return null;
    }

    return file.CreateReadStream();
  }
}