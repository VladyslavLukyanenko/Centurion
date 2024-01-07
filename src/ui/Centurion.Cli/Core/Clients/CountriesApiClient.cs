using System.Net.Http.Json;
using Centurion.Cli.Core.Domain.Profiles;

namespace Centurion.Cli.Core.Clients;

public class CountriesApiClient : ICountriesApiClient
{
  private readonly HttpClient _client;

  public CountriesApiClient(HttpClient client)
  {
    _client = client;
  }

  public async Task<IList<Country>> GetCountriesAsync(string licenseKey, CancellationToken ct = default)
  {
    return await _client.GetFromJsonAsync<IList<Country>>("/v1/countries", ct) ?? Array.Empty<Country>();
  }
}