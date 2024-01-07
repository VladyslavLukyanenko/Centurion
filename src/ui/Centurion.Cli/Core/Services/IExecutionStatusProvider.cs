namespace Centurion.Cli.Core.Services;

public interface IExecutionStatusProvider
{
  IObservable<bool> IsFetching { get; }
}