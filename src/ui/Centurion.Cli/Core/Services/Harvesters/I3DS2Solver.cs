using Centurion.Contracts.Checkout;

namespace Centurion.Cli.Core.Services.Harvesters;

public interface I3DS2Solver : IAsyncDisposable
{
  ValueTask<Solve3DS2CommandReply> Solve(Solve3DS2Command solveParams);
  event EventHandler Terminated;
}