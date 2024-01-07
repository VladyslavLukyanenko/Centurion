using Centurion.Accounts.App.Services;
using Centurion.Accounts.Core.FileStorage.Config;
using Centurion.Accounts.Core.FileStorage.FileSystem;
using Centurion.Accounts.Infra.Services.Cryptographic;

namespace Centurion.Accounts.Infra.Services.FileSystem;

public class FileUploadService : IFileUploadService
{
  private readonly ICryptographicService _cryptographicService;
  private readonly IFileSystemService _fileSystemService;
  private readonly IPathsService _pathsService;
  private readonly IBinaryDataProcessingPipelineProvider _processingPipelineProvider;

  public FileUploadService(IPathsService pathsService,
    IBinaryDataProcessingPipelineProvider processingPipelineProvider,
    ICryptographicService cryptographicService, IFileSystemService fileSystemService)
  {
    _pathsService = pathsService;
    _processingPipelineProvider = processingPipelineProvider;
    _cryptographicService = cryptographicService;
    _fileSystemService = fileSystemService;
  }

  public Task<StoredBinaryData> StoreAsync(IBinaryData data, IEnumerable<FileUploadsConfig> configs,
    string? oldFileName = null, CancellationToken ct = default)
  {
    return StoreAsync(data, configs, new RandomFileNameProvider(oldFileName), ct);
  }

  public async Task<StoredBinaryData> StoreAsync(IBinaryData data, IEnumerable<FileUploadsConfig> configs,
    IFileNameProvider fileNameProvider, CancellationToken ct = default)
  {
    var config = configs.FirstOrDefault(cfg => cfg.IsFileTypeSupported(data.ContentType));
    if (config == null)
    {
      throw new NotSupportedException("NotSupportedUploadFileType");
    }

    await TryRemoveFileAsync(config, fileNameProvider);

    var storeRoot = _pathsService.GetStoreAbsolutePath(config.StoreName);

    var pipeline = _processingPipelineProvider.GetPipeline(config, data);
    if (pipeline != null)
    {
      data = await pipeline.ProcessAsync(config, data, ct);
    }

    using (var fileStream = data.OpenReadStream())
    {
      var computedHash = config.AppendHashToFileName
        ? await _cryptographicService.ComputeHashAsync(fileStream)
        : string.Empty;

      var destFileName = fileNameProvider.GetDstFileName(config, data, computedHash);
      var storeFullPath =
        await _fileSystemService.SaveBinaryAsync(storeRoot, destFileName, fileStream, ct);
      var relativePath = _pathsService.ToServerRelative(storeFullPath);
      var extension = data.GetExtension();

      var storeResult = new StoreFileResult(storeFullPath, relativePath, computedHash, destFileName,
        extension);
      return new StoredBinaryData(data, storeResult);
    }
  }

  private async Task TryRemoveFileAsync(FileUploadsConfig config, IFileNameProvider fileNameProvider)
  {
    if (string.IsNullOrEmpty(fileNameProvider.OldFileName))
    {
      return;
    }

    var fullPath = _pathsService.GetStoreAbsolutePath(config.StoreName, fileNameProvider.OldFileName);
    if (File.Exists(fullPath))
    {
      await Task.Run(() => File.Delete(fullPath));
    }
  }
}