using System.Reactive.Linq;
using System.Reactive.Subjects;
using Centurion.Contracts.TaskManager;

namespace Centurion.Cli.Core.Services.Tasks;

public class OrchestratorService : IOrchestratorService
{
  private readonly Subject<OrchestratorCommand> _command = new();
  public IObservable<OrchestratorCommand> Commands => _command.AsObservable();

  public void Send(OrchestratorCommand cmd)
  {
    _command.OnNext(cmd);
  }
}