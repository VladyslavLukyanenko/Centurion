using Centurion.Accounts.Core.FileStorage.Config;

namespace Centurion.Accounts.Core.FileStorage.FileSystem;

public interface IFileNameProvider
{
  string? OldFileName { get; }
  string GetDstFileName(FileUploadsConfig cfg, IBinaryData data, string? computedHash = null);
}