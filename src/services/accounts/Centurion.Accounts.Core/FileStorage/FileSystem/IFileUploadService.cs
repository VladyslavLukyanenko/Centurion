using Centurion.Accounts.Core.FileStorage.Config;

namespace Centurion.Accounts.Core.FileStorage.FileSystem;

public interface IFileUploadService
{
  Task<StoredBinaryData> StoreAsync(IBinaryData data, IEnumerable<FileUploadsConfig> configs,
    string? oldFileName = null, CancellationToken ct = default);

  Task<StoredBinaryData> StoreAsync(IBinaryData data, IEnumerable<FileUploadsConfig> configs,
    IFileNameProvider fileNameProvider, CancellationToken ct = default);
}

public static class IFileUploadServiceExtensions
{
  public static async ValueTask<string> UploadFileOrDefaultAsync(this IFileUploadService fileUploadService,
    IBinaryData? binaryData, FileUploadsConfig cfg, string? defaultValue, CancellationToken ct = default)
  {
    if (binaryData == null || binaryData.Length == 0)
    {
      return defaultValue!;
    }

    var oldFileName = string.IsNullOrEmpty(defaultValue) ? null : Path.GetFileName(defaultValue);
    var result = await fileUploadService.StoreAsync(binaryData, new[] {cfg}, oldFileName, ct);
    return result.StoreFileResult.RelativeFilePath;
  }
}