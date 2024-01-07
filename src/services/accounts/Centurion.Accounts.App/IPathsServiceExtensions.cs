using Centurion.Accounts.App.Services;

namespace Centurion.Accounts.App;

// ReSharper disable once InconsistentNaming
public static class IPathsServiceExtensions
{
  public static string? GetAbsoluteImageUrl(this IPathsService pathsService, string? relativePath,
    string? fallbackRelativePath = null)
  {
    if (string.IsNullOrEmpty(relativePath))
    {
      relativePath = fallbackRelativePath;
    }

    return pathsService.ToAbsoluteUrl(relativePath);
  }
}