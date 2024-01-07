namespace Centurion.Cli.Core.Services;

public interface ILicenseKeyStore
{
  ValueTask<string?> GetStoredKeyAsync(CancellationToken ct = default);
  ValueTask StoreKeyAsync(string key, CancellationToken ct = default);
}