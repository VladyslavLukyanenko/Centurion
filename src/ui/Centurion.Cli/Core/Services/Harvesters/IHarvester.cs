using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Accounts;
using CSharpFunctionalExtensions;

namespace Centurion.Cli.Core.Services.Harvesters;

public interface IHarvester : IAsyncDisposable
{
  ValueTask<Result> Start(InitializedHarvesterModel harvester, CancellationToken ct);

  ValueTask<string> TrySolveCaptcha(string url, TimeSpan? timeout = null);
  IObservable<int> TokensHarvested { get; }

  public Proxy Proxy { get; }
  public Account Account { get; }
  public HarvesterModel Harvester { get; }
  bool IsInitialized { get; }
  event EventHandler Terminated;
}