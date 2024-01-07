using Centurion.Accounts.App.Services;
using Newtonsoft.Json;

namespace Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;

public class UploadedFilePathToAbsoluteUrlJsonConverter : JsonConverter
{
  private readonly Func<string?> _fallbackPicturePathProvider;
  private readonly IPathsService _pathsService;

  public UploadedFilePathToAbsoluteUrlJsonConverter(Func<string?> fallbackPicturePathProvider,
    IPathsService pathsService)
  {
    _fallbackPicturePathProvider = fallbackPicturePathProvider;
    _pathsService = pathsService;
  }

  public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
  {
    var absoluteUrl = _pathsService.GetAbsoluteImageUrl(value as string, _fallbackPicturePathProvider());
    writer.WriteValue(absoluteUrl);
  }

  public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
    JsonSerializer serializer)
  {
    return _pathsService.ToRelativeUrl(reader.Value?.ToString());
  }

  public override bool CanConvert(Type objectType) => objectType == typeof(string);
}