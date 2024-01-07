using System.Text.RegularExpressions;
using Centurion.Accounts.Core.Products.Config;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Products;

public class HostingConfig
{
  public string DomainName { get; set; } = null!;
  public DashboardHostingMode Mode { get; set; }

  public static Result<IDictionary<DashboardHostingMode, string>> ResolvePossibleModes(
    string rawDashboardLocation, DashboardsConfig config)
  {
    if (!Uri.TryCreate(rawDashboardLocation, UriKind.RelativeOrAbsolute, out var url)
        || !url.IsAbsoluteUri
        || string.IsNullOrEmpty(url.AbsolutePath))
    {
      return Result.Failure<IDictionary<DashboardHostingMode, string>>("Invalid dashboard url provided");
    }

    var result = new Dictionary<DashboardHostingMode, string>();
    var segmentMatch = Regex.Match(url.AbsolutePath, config.LocationPathSegmentRegex);
    if (segmentMatch.Success)
    {
      result[DashboardHostingMode.PathSegment] = segmentMatch.Groups[1].Value;
    }

    if (url.HostNameType == UriHostNameType.Dns)
    {
      if (url.Host.Count(c => c == '.') > 1)
      {
        var dotIDx = url.Host.IndexOf('.');
        result[DashboardHostingMode.Subdomain] = url.Host[..dotIDx];
      }
      else
      {
        result[DashboardHostingMode.Dedicated] = url.Host;
      }
    }

    return result;
  }
}