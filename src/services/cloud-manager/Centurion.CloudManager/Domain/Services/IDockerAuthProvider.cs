using Docker.DotNet.Models;

namespace Centurion.CloudManager.Domain.Services;

public interface IDockerAuthProvider
{
  ValueTask<AuthConfig> GetAuthConfigAsync(CancellationToken ct = default);
}