namespace Centurion.Accounts.Core.FileStorage.FileSystem;

public class StoredBinaryData
  : IBinaryData
{
  private readonly IBinaryData _source;

  public StoredBinaryData(IBinaryData source, StoreFileResult storeFileResult)
  {
    StoreFileResult = storeFileResult;
    _source = source;
  }

  public StoreFileResult StoreFileResult { get; }

  public string GetNameWithoutExtension()
  {
    return _source.GetNameWithoutExtension();
  }

  public string GetExtension()
  {
    return _source.GetExtension();
  }

  public string Name => _source.Name;

  public string ContentType => _source.ContentType;

  public long Length => _source.Length;

  public Stream OpenReadStream()
  {
    return _source.OpenReadStream();
  }
}