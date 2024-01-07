using Centurion.Accounts.Core.FileStorage.Config;
using Centurion.Accounts.Core.FileStorage.FileSystem;

namespace Centurion.Accounts.Infra.Services.FileSystem;

public abstract class FileNameProvider
  : IFileNameProvider
{
  private const string HashDelimiter = ".";

  protected FileNameProvider(string? oldFileName)
  {
    OldFileName = oldFileName;
  }

  public string? OldFileName { get; }

  public string GetDstFileName(FileUploadsConfig cfg, IBinaryData data, string? computedHash = null)
  {
    var fileName = GetDstFileNameWithoutExt(cfg, data);
    var maybeHash = string.IsNullOrEmpty(computedHash)
      ? string.Empty
      : HashDelimiter + computedHash;

    return string.Concat(fileName, maybeHash, data.GetExtension());
  }

  protected abstract string GetDstFileNameWithoutExt(FileUploadsConfig cfg, IBinaryData data);
}