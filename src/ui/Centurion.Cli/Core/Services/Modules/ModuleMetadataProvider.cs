using Centurion.Cli.Core.Modules;
using Centurion.Contracts;
using Centurion.Contracts.TaskManager;
using CSharpFunctionalExtensions;
using Google.Protobuf.WellKnownTypes;

namespace Centurion.Cli.Core.Services.Modules;

public class ModuleMetadataProvider : ExecutionStatusProviderBase, IModuleMetadataProvider
{
  private readonly Orchestrator.OrchestratorClient _orchestratorClient;
  private List<ModuleMetadata> _modules = new();

  public ModuleMetadataProvider(Orchestrator.OrchestratorClient orchestratorClient)
  {
    _orchestratorClient = orchestratorClient;
  }

  public IReadOnlyList<ModuleMetadata> SupportedModules => _modules.AsReadOnly();

  public async ValueTask<Result> InitializeAsync(CancellationToken ct = default)
  {
    var list = await _orchestratorClient.GetSupportedModulesAsync(new Empty(), cancellationToken: ct)
      .TrackProgress(FetchingTracker);
    ResetCache();
    _modules.AddRange(list.Modules);
    return Result.Success();
  }

  public void ResetCache()
  {
    _modules.Clear();
  }
}