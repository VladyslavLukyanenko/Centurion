using Centurion.Cli.Core.Domain.Profiles;

namespace Centurion.Cli.Core.Services;

public interface ICountriesService : IAppStateHolder
{
  IReadOnlyList<Country> Countries { get; }
  public string? GetCountryName(string id) => Countries.FirstOrDefault(_ => _.Id == id)?.Title;
  public string? GetProvinceName(string countryId, string provinceId)
  {
    var country = Countries.FirstOrDefault(_ => _.Id == countryId);
    return country?.Provinces.FirstOrDefault(_ => _.Code == provinceId)?.Title;
  }
}