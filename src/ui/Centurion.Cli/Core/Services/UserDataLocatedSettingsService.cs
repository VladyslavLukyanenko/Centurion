using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Centurion.Cli.Core.Services;

public class UserDataLocatedSettingsService : ISettingsService
{
  private readonly ILogger<UserDataLocatedSettingsService> _logger;

  private static readonly SemaphoreSlim ReadSemaphore = new(1, 1);
  private static readonly SemaphoreSlim WriteSemaphore = new(1, 1);

  public UserDataLocatedSettingsService(ILogger<UserDataLocatedSettingsService> logger)
  {
    _logger = logger;
  }

  public async ValueTask<T?> ReadSettingsOrDefaultAsync<T>(string name, Func<T>? defaultFactory = null,
    CancellationToken ct = default)
  {
    try
    {
      await ReadSemaphore.WaitAsync(ct);
      var fullPath = GetSettingsFullPathOrDefault(name);
      _logger.LogDebug("Reading settings from path '{FullPath}' by key '{Key}'", fullPath, name);
      if (!File.Exists(fullPath))
      {
        _logger.LogDebug("File not found. Returning default value");
        return defaultFactory != null ? defaultFactory.Invoke()! : default;
      }

      _logger.LogDebug("File found. Reading...");
      var content = await File.ReadAllTextAsync(fullPath, ct);

      _logger.LogDebug("Deserializing");
      return JsonConvert.DeserializeObject<T>(content);
    }
    finally
    {
      _logger.LogDebug("Read finished");
      ReadSemaphore.Release();
    }
  }

  public async ValueTask WriteSettingsAsync<T>(string name, T settings, CancellationToken ct = default)
  {
    try
    {
      await WriteSemaphore.WaitAsync(ct);
      var fullPath = GetSettingsFullPathOrDefault(name);
      var json = JsonConvert.SerializeObject(settings);
      await File.WriteAllTextAsync(fullPath, json, ct);
    }
    finally
    {
      WriteSemaphore.Release();
    }
  }

  private string GetSettingsFullPathOrDefault(string name)
  {
    if (!Directory.Exists(AppInfo.StorageLocation))
    {
      _logger.LogDebug("Store location doesn't exists. Creating '{StorageLocation}'", AppInfo.StorageLocation);
      Directory.CreateDirectory(AppInfo.StorageLocation);
      _logger.LogDebug("Store location created");
    }

    return Path.Combine(AppInfo.StorageLocation, name);
  }
}