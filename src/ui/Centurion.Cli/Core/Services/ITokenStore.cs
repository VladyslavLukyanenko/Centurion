namespace Centurion.Cli.Core.Services;

public interface ITokenStore : ITokenProvider
{
  void UseToken(string accessToken);
}