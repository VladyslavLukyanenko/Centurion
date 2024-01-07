using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.Evga
{
  public class EvgaFetcher : IProductStatusFetcher
  {
    private static readonly Regex AvailabilityRegex =
      new("<span class=\"AddToChat\">ADD TO CART</span>", RegexOptions.Compiled);

    private readonly HttpClient _httpClient;
    private readonly Uri _productUrl;

    public EvgaFetcher(HttpClient httpClient, Uri productUrl)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      _httpClient = httpClient;
      _productUrl = productUrl;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, _productUrl);
      return await StatusFetchResult.ProcessResult(request, _httpClient, ct, result =>
      {
        var content = result.GetResponseAsString();
        var available = AvailabilityRegex.IsMatch(content);

        result.AddStatus(_productUrl.ToString(), available);

        return result;
      });
    }
  }
}