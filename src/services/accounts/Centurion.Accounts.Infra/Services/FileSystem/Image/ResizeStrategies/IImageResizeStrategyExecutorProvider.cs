using Centurion.Accounts.Core.FileStorage.Image.ResizeStrategies;

namespace Centurion.Accounts.Infra.Services.FileSystem.Image.ResizeStrategies;

public interface IImageResizeStrategyExecutorProvider
{
  IImageResizeStrategyExecutor? Get(ImageResizeStrategy strategy);
}