using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Domain;
using CSharpFunctionalExtensions;
using LiteDB;

namespace Centurion.Cli.Core.Services;

public class GeneralSettingsService : IGeneralSettingsService
{
  private static readonly string CustomSoundsSavePath = Path.Combine(AppInfo.StorageLocation, "CustomSounds");

  private static readonly SemaphoreSlim Lock = new(1, 1);
  private readonly ILiteDatabase _database;
  private readonly BehaviorSubject<GeneralSettings> _settings = new(new GeneralSettings());
  private readonly ILiteCollection<GeneralSettings> _collection;
  private readonly IAudioService _audioService;
  private readonly GeneralSettingsConfig _config;

  public GeneralSettingsService(ILiteDatabase database, IAudioService audioService, GeneralSettingsConfig config)
  {
    _database = database;
    _audioService = audioService;
    _config = config;
    _collection = database.GetCollection<GeneralSettings>();
    Settings = _settings.AsObservable();
  }

  public IObservable<GeneralSettings> Settings { get; }

  public ValueTask Save(GeneralSettings settings)
  {
    _collection.Update(settings);
    _database.Checkpoint();
    _settings.OnNext(settings);
    return default;
  }

  public async ValueTask UpdateCheckoutSound(string filePath, CancellationToken ct = default)
  {
    _settings.Value.CheckoutSoundMp3FilePath = await SaveCustomSoundFile(filePath, ct);
  }

  public async ValueTask UpdateDeclineSound(string filePath, CancellationToken ct = default)
  {
    _settings.Value.DeclineSoundMp3FilePath = await SaveCustomSoundFile(filePath, ct);
  }

  public ValueTask PlayCheckoutSound(CancellationToken ct = default)
  {
    if (string.IsNullOrEmpty(_settings.Value.CheckoutSoundMp3FilePath))
    {
      return default;
    }

    return _audioService.Play(_settings.Value.CheckoutSoundMp3FilePath);
  }

  public ValueTask PlayDeclineSound(CancellationToken ct = default)
  {
    if (string.IsNullOrEmpty(_settings.Value.DeclineSoundMp3FilePath))
    {
      return default;
    }

    return _audioService.Play(_settings.Value.DeclineSoundMp3FilePath);
  }

  public void ResetCheckoutSound()
  {
    _settings.Value.CheckoutSoundMp3FilePath = _config.CheckoutSoundFilePath;
  }

  public void ResetDeclineSound()
  {
    _settings.Value.DeclineSoundMp3FilePath = _config.DeclineSoundFilePath;
  }

  public IObservable<bool> IsFetching { get; } = Observable.Return(false);

  public ValueTask<Result> InitializeAsync(CancellationToken ct = default)
  {
    var allSettings = _collection.FindAll();
    GeneralSettings? settings = allSettings.FirstOrDefault();
    if (settings is null)
    {
      settings = new GeneralSettings
      {
        CheckoutSoundMp3FilePath = _config.CheckoutSoundFilePath,
        DeclineSoundMp3FilePath = _config.DeclineSoundFilePath,
        CheckoutSoundEnabled = true,
        DeclineSoundEnabled = true
      };

      _collection.Insert(settings);
      _database.Checkpoint();
    }

    _settings.OnNext(settings);
    return ValueTask.FromResult(Result.Success());
  }

  public void ResetCache()
  {
    _settings.OnNext(new GeneralSettings());
  }

  private static async Task<string> SaveCustomSoundFile(string filePath, CancellationToken ct)
  {
    var fileName = Path.GetFileName(filePath);
    var copyFullPath = Path.Combine(CustomSoundsSavePath, fileName);
    if (!Directory.Exists(CustomSoundsSavePath))
    {
      try
      {
        await Lock.WaitAsync(CancellationToken.None);
        if (!Directory.Exists(CustomSoundsSavePath))
        {
          Directory.CreateDirectory(CustomSoundsSavePath);
        }
      }
      finally
      {
        Lock.Release();
      }
    }

    await using var input = new FileStream(filePath, FileMode.Open);
    await using var output = File.OpenWrite(copyFullPath);
    await input.CopyToAsync(output, ct);
    await input.FlushAsync(ct);
    return copyFullPath;
  }
}