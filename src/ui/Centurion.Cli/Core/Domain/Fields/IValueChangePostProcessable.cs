using Splat;

namespace Centurion.Cli.Core.Domain.Fields;

public interface IValueChangePostProcessable
{
  Task PostProcessAsync(IReadonlyDependencyResolver dependencyResolver);
}