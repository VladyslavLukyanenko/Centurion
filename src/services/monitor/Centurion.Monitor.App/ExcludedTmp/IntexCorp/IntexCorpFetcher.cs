using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.IntexCorp
{
  public class IntexCorpFetcher : IProductStatusFetcher
  {
    private static readonly Regex AvailabilityRegex =
      new("<meta property=\"og:availability\" content=\"([^\"]*)\" />", RegexOptions.Compiled);

    private readonly HttpClient _httpClient;
    private readonly Uri _productUrl;

    public IntexCorpFetcher(HttpClient httpClient, Uri productUrl)
    {
      httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
      httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
      _httpClient = httpClient;
      _productUrl = productUrl;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, _productUrl);
      return await StatusFetchResult.ProcessResult(request, _httpClient, ct, result =>
      {
        var content = result.GetResponseAsString();
        var available = AvailabilityRegex.Match(content).Groups[1].Value == "In stock";

        result.AddStatus(_productUrl.ToString(), available);

        return result;
      });
    }
  }
}