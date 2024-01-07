namespace Centurion.CloudManager.Domain.Services;

public interface IDockerManager
{
  Task<bool> IsRemoteApiAvailableAsync(Uri serverEndpoint, CancellationToken ct = default);
  Task<string> CreateImageAsync(string image, CancellationToken ct = default);
}