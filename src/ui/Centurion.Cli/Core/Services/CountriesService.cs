using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Centurion.Cli.Core.Clients;
using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Services.Modules;
using CSharpFunctionalExtensions;

namespace Centurion.Cli.Core.Services;

public class CountriesService : ExecutionStatusProviderBase, ICountriesService
{
  private readonly ILicenseKeyProvider _licenseKeyProvider;
  private readonly ICountriesApiClient _countriesApiClient;

  public CountriesService(ILicenseKeyProvider licenseKeyProvider, ICountriesApiClient countriesApiClient)
  {
    _licenseKeyProvider = licenseKeyProvider;
    _countriesApiClient = countriesApiClient;
  }

  public ValueTask<Result> InitializeAsync(CancellationToken ct = default)
  {
    _licenseKeyProvider.LicenseKey
      .Where(key => !string.IsNullOrEmpty(key))
      .Take(1)
      .Select(key => _countriesApiClient.GetCountriesAsync(key!, ct).TrackProgress(FetchingTracker).ToObservable())
      .Switch()
      .Subscribe(countries =>
      {
        Countries = countries.ToList();
      });

    return default;
  }

  public void ResetCache()
  {
    Countries = Array.Empty<Country>();
  }

  public IReadOnlyList<Country> Countries { get; private set; } = Array.Empty<Country>();
}