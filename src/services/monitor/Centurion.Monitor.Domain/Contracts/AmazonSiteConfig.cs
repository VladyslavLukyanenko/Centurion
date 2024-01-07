

// ReSharper disable once CheckNamespace
namespace Centurion.Contracts.Checkout.Amazon;

public partial class AmazonConfig
{
  public const string DefaultRegion = "USA";
  private static readonly Random Rnd = new((int)DateTime.Now.Ticks);

  private static readonly IDictionary<string, string> RegionsDict = new Dictionary<string, string>
  {
    { DefaultRegion, ".com" },
    { "CA", ".ca" },
    { "UK", ".co.uk" },
    { "NL", ".nl" },
    { "FR", ".fr" },
    { "IT", ".it" },
    { "DE", ".de" },
    { "JP", ".co.jp" },
  };

  private static readonly IDictionary<string, Uri> SitesDict =
    RegionsDict.ToDictionary(_ => _.Key, _ => new Uri($"https://www.amazon{_.Value}"));

  private static readonly IDictionary<string, Uri[]> DomainPools = new Dictionary<string, Uri[]>
  {
    {
      DefaultRegion, new[]
      {
        new Uri("https://music.amazon.com/"),
        new Uri("https://www.amazon.com/"),
        new Uri("https://prime.amazon.com/")
      }
    }
  };

  public static Uri HomePageUrl => SitesDict[DefaultRegion];

  public Uri BaseUrl
  {
    get
    {
      if (SitesDict.TryGetValue(Region, out var url))
      {
        return url;
      }

      return SitesDict[DefaultRegion];
    }
  }

  public Uri GetRandomDomainUrl()
  {
    if (DomainPools.TryGetValue(Region, out var pool))
    {
      var ix = Rnd.Next(pool.Length);
      return pool[ix];
    }

    return BaseUrl;
  }
}