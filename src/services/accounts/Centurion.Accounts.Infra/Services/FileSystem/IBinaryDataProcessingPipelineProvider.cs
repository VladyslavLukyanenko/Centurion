using Centurion.Accounts.Core.FileStorage.Config;
using Centurion.Accounts.Core.FileStorage.FileSystem;

namespace Centurion.Accounts.Infra.Services.FileSystem;

public interface IBinaryDataProcessingPipelineProvider
{
  IBinaryDataProcessingPipeline? GetPipeline(FileUploadsConfig cfg, IBinaryData data);
}