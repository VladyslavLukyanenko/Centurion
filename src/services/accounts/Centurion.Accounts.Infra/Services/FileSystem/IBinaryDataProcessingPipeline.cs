using Centurion.Accounts.Core.FileStorage.Config;
using Centurion.Accounts.Core.FileStorage.FileSystem;

namespace Centurion.Accounts.Infra.Services.FileSystem;

public interface IBinaryDataProcessingPipeline
{
  bool CanProcess(FileUploadsConfig fileCfg, IBinaryData data);

  Task<IBinaryData> ProcessAsync(FileUploadsConfig fileCfg, IBinaryData data, CancellationToken token = default);
}