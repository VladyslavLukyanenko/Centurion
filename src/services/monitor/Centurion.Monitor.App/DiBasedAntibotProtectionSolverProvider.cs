using Centurion.Monitor.Domain.Antibot;

namespace Centurion.Monitor.App;

public class DiBasedAntibotProtectionSolverProvider : IAntibotProtectionSolverProvider
{
  private readonly IEnumerable<IAntibotProtectionSolver> _solvers;

  public DiBasedAntibotProtectionSolverProvider(IEnumerable<IAntibotProtectionSolver> solvers)
  {
    _solvers = solvers;
  }

  public IAntibotProtectionSolver? GetSolver(string provider)
  {
    return _solvers.FirstOrDefault(_ =>
      string.Equals(_.ProviderName, provider, StringComparison.InvariantCultureIgnoreCase));
  }
}