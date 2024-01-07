using Centurion.Cli.Core.Services.Harvesters;
using Splat;

namespace Centurion.Cli.Core.Services;

public class DiBasedHarvesterFactory : IHarvesterFactory
{
  private readonly IReadonlyDependencyResolver _resolver;

  public DiBasedHarvesterFactory(IReadonlyDependencyResolver resolver)
  {
    _resolver = resolver;
  }

  public IHarvester Create() => _resolver.GetService<IHarvester>()!;
}