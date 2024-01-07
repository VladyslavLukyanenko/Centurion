using Docker.DotNet.Models;
using Centurion.CloudManager.Domain.Services;

namespace Centurion.CloudManager.App.Services;

public class LoginPwdDockerAuthProvider : IDockerAuthProvider
{
  private readonly LoginPwdDockerAuthConfig _config;

  public LoginPwdDockerAuthProvider(LoginPwdDockerAuthConfig config)
  {
    _config = config;
  }

  public ValueTask<AuthConfig> GetAuthConfigAsync(CancellationToken ct = default)
  {
    var auth = new AuthConfig
    {
      Username = _config.Username,
      Password = _config.Password
    };

    return ValueTask.FromResult(auth);
  }
}