using Centurion.Cli.Core.Services;
using Centurion.Contracts;

namespace Centurion.Cli.Core.Modules;

public interface IModuleMetadataProvider : IAppStateHolder
{
  IReadOnlyList<ModuleMetadata> SupportedModules { get; }
}