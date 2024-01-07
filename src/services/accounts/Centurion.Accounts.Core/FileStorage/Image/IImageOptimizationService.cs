using Centurion.Accounts.Core.FileStorage.FileSystem;

namespace Centurion.Accounts.Core.FileStorage.Image;

public interface IImageOptimizationService
{
  Task<IBinaryData> OptimizeAsync(OptimizationImageContext context, CancellationToken token = default);
}