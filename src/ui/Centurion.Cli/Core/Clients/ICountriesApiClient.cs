using Centurion.Cli.Core.Domain.Profiles;

namespace Centurion.Cli.Core.Clients;

public interface ICountriesApiClient
{
  Task<IList<Country>> GetCountriesAsync(string licenseKey, CancellationToken ct = default);
}