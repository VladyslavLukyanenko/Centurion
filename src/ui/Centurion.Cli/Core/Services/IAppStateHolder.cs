using CSharpFunctionalExtensions;

namespace Centurion.Cli.Core.Services;

public interface IAppStateHolder : IExecutionStatusProvider
{
  ValueTask<Result> InitializeAsync(CancellationToken ct = default);
  void ResetCache();
}