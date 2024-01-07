namespace Centurion.Monitor.Domain.Antibot;

public interface IAntibotProtectionSolverProvider
{
  IAntibotProtectionSolver? GetSolver(string provider);
}