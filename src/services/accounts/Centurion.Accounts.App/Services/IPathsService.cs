using System.Diagnostics.CodeAnalysis;

namespace Centurion.Accounts.App.Services;

public interface IPathsService
{
  [return: NotNullIfNotNull("url")]
  string? ToRelativeUrl(string? url);

  string ToServerRelative(string path);
  string? ToAbsoluteUrl(string? path);
  string GetStoreAbsolutePath(params string[] segments);
  string GetAbsolutePathFromUrl(string url);
  string GetPhysicalPath(string path);
}