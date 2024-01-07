namespace Centurion.Cli.Core.Services;

public class LicenseKeyStore : ILicenseKeyStore
{
  private static readonly string SettingsFileName = "Key." + AppInfo.ProductTechName.ToLowerInvariant();
  private readonly ISettingsService _settingsService;

  public LicenseKeyStore(ISettingsService settingsService)
  {
    _settingsService = settingsService;
  }

  public ValueTask<string?> GetStoredKeyAsync(CancellationToken ct = default)
  {
    return _settingsService.ReadSettingsOrDefaultAsync<string>(SettingsFileName, ct: ct);
  }

  public ValueTask StoreKeyAsync(string key, CancellationToken ct = default)
  {
    return _settingsService.WriteSettingsAsync(SettingsFileName, key, ct);
  }
}