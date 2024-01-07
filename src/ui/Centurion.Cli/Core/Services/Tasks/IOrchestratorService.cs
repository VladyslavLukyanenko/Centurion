using Centurion.Contracts.TaskManager;

namespace Centurion.Cli.Core.Services.Tasks;

public interface IOrchestratorService
{
  IObservable<OrchestratorCommand> Commands { get; }
  void Send(OrchestratorCommand cmd);
}