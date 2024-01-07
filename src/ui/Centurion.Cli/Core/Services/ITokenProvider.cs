namespace Centurion.Cli.Core.Services;

public interface ITokenProvider
{
  IObservable<string?> AccessToken { get; }
  string? CurrentAccessToken { get; }
}