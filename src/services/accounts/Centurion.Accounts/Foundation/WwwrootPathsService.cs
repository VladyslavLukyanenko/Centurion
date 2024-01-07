using System.Diagnostics.CodeAnalysis;
using Centurion.Accounts.App.Config;
using Centurion.Accounts.App.Services;

namespace Centurion.Accounts.Foundation;

public class WwwrootPathsService : IPathsService
{
  private readonly CommonConfig _commonConfig;
  private readonly IWebHostEnvironment _environment;

  public WwwrootPathsService(IWebHostEnvironment environment, CommonConfig commonConfig)
  {
    _environment = environment;
    _commonConfig = commonConfig;
  }

  public string? ToAbsoluteUrl(string? path)
  {
    if (string.IsNullOrEmpty(path))
    {
      return null;
    }

    if (path.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
        || path.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
    {
      return path;
    }

    var hostWithScheme = _commonConfig.HostingInfo.HostName;
    if (!path.StartsWith("/") && !hostWithScheme.EndsWith("/"))
    {
      hostWithScheme += "/";
    }

    return hostWithScheme + path;
  }

  public string GetStoreAbsolutePath(params string[] segments)
  {
    var allSegments = new List<string>();
    if (!_commonConfig.Uploads.IsPathRelative)
    {
      allSegments.Add(_commonConfig.Uploads.DirectoryName);
    }

    allSegments.AddRange(segments.Select(SanitizePathSegment));

    var storePath = Path.Combine(allSegments.ToArray());
    if (Path.IsPathRooted(storePath))
    {
      return storePath;
    }

    return Path.Combine(_environment.WebRootPath, storePath);
  }

  public string GetAbsolutePathFromUrl(string url)
  {
    var uri = new Uri(url);
    var path = uri.AbsolutePath;
    var info = _environment.WebRootFileProvider.GetFileInfo(path);

    return info.PhysicalPath;
  }

  public string GetPhysicalPath(string path)
  {
    var relativePath = SanitizePathSegment(path);

    return Path.Combine(_environment.WebRootPath, relativePath);
  }

  [return: NotNullIfNotNull("url")]
  public string? ToRelativeUrl(string? url)
  {
    if (string.IsNullOrEmpty(url))
    {
      return null;
    }

    if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var parsedUrl))
    {
      return url;
    }

    if (!parsedUrl.IsAbsoluteUri)
    {
      return url;
    }

    if (!string.Equals(parsedUrl.Host, _commonConfig.HostingInfo.DomainName))
    {
      return url;
    }

    var path = parsedUrl.AbsolutePath;
    var info = _environment.WebRootFileProvider.GetFileInfo(path);
    if (!info.Exists || info.IsDirectory)
    {
      return url;
    }

    return ToServerRelative(info.PhysicalPath);
  }

  public string ToServerRelative(string path)
  {
    var relativePath = path.Replace(_environment.WebRootPath, string.Empty);
    var normalizedRelativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
    if (!normalizedRelativePath.StartsWith("/"))
    {
      normalizedRelativePath = "/" + normalizedRelativePath;
    }

    return normalizedRelativePath;
  }

  private static string SanitizePathSegment(string relativePath)
  {
    if (relativePath.StartsWith("/"))
    {
      relativePath = relativePath.Substring(1);
    }

    return relativePath;
  }
}