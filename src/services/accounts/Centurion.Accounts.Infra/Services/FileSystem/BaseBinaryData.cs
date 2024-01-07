using Centurion.Accounts.Core.FileStorage.FileSystem;

namespace Centurion.Accounts.Infra.Services.FileSystem;

public abstract class BaseBinaryData
  : IBinaryData
{
  public string GetNameWithoutExtension()
  {
    return Path.GetFileNameWithoutExtension(Name);
  }

  public string GetExtension()
  {
    return Path.GetExtension(Name);
  }

  public string Name { get; set; } = null!;
  public string ContentType { get; set; } = null!;
  public long Length { get; set; }
  public abstract Stream OpenReadStream();
}