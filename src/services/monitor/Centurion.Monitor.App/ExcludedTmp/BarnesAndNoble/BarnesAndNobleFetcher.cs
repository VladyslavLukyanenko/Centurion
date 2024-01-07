using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Centurion.Monitor.Domain;
using Centurion.Monitor.Domain.Services;

namespace Centurion.Monitor.App.Sites.BarnesAndNoble
{
  public class BarnesAndNobleFetcher : IProductStatusFetcher
  {
    private static readonly Regex JsonRegex =
      new("<div class=\"analytics-qv-data hidden\">([^<]*)</div>", RegexOptions.Compiled);

    private readonly HttpClient _httpClient;
    private readonly Uri _productUrl;
    private readonly IJsonSerializer _jsonSerializer;

    public BarnesAndNobleFetcher(HttpClient httpClient, Uri productUrl, IJsonSerializer jsonSerializer)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      _httpClient = httpClient;
      _productUrl = productUrl;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, _productUrl);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var matches = JsonRegex.Matches(result.GetResponseAsString());
        var products = new List<BarnesAndNobleData>();
        foreach (Match match in matches)
        {
          var prod = await _jsonSerializer.DeserializeAsync<BarnesAndNobleData>(match.Value, ct);
          products.Add(prod!);
        }


        // todo: fix. it wasn't implemented correctly in golang
        var available = products.Any(_ => !_.Product.Attributes.OutOfStock);
        result.AddStatus(_productUrl.ToString(), available);

        return result;
      });
    }
  }
}