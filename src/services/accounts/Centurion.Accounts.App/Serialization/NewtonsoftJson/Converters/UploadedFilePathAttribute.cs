namespace Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;

public class UploadedFilePathAttribute : DataConverterAttributeBase
{
  public UploadedFilePathAttribute(string? fallbackPictureKey = null)
  {
    FallbackPictureKey = fallbackPictureKey;
  }

  public string? FallbackPictureKey { get; private set; }
}