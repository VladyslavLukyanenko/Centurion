namespace Centurion.Accounts.Infra.Services.FileSystem;

public class BinaryData
  : BaseBinaryData
{
  private readonly Func<Stream> _contentProvider;

  public BinaryData(string name, Func<Stream> contentProvider, string contentType, long length)
  {
    _contentProvider = contentProvider;
    Name = name;
    Length = length;
    ContentType = contentType;
  }

  public override Stream OpenReadStream()
  {
    return _contentProvider();
  }
}