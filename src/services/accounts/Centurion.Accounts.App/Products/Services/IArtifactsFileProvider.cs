using CSharpFunctionalExtensions;
using Microsoft.Extensions.FileProviders;

namespace Centurion.Accounts.App.Products.Services;

public interface IArtifactsFileProvider : IFileProvider
{
  Stream? TryOpenStreamOfVersion(Guid dashboardId, string channel, string os, string arch, Version version, string ext);
  Maybe<Version?> GetLatestVersion(Guid dashboardId, string channel, string os, string arch);
}