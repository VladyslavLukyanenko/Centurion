using Docker.DotNet;
using Docker.DotNet.Models;
using Centurion.CloudManager.Domain.Services;

namespace Centurion.CloudManager.App.Services;

// todo: remove this class
public class DockerManager : IDockerManager
{
  private readonly IDockerClientsProvider _dockerClientsProvider;
  private readonly IDockerAuthProvider _authProvider;

  public DockerManager(IDockerClientsProvider dockerClientsProvider, IDockerAuthProvider authProvider)
  {
    _dockerClientsProvider = dockerClientsProvider;
    _authProvider = authProvider;
  }

  public async Task<bool> IsRemoteApiAvailableAsync(Uri serverEndpoint, CancellationToken ct = default)
  {
    DockerClient client = new DockerClientConfiguration(serverEndpoint).CreateClient();
    try
    {
      await client.System.PingAsync(ct);
      return true;
    }
    catch
    {
      return false;
    }
  }

  public async Task<string> CreateImageAsync(string image, CancellationToken ct = default)
  {
    var client = _dockerClientsProvider.GetLocalDockerClient();
      
    await client.Images.CreateImageAsync(new ImagesCreateParameters
      {
        FromImage = image
      },
      await _authProvider.GetAuthConfigAsync(ct),
      new Progress<JSONMessage>(),
      ct);

    var images = await client.Images.ListImagesAsync(new ImagesListParameters {All = true}, ct);
    return images.Where(_ => _.RepoTags.Any(d => d == image))
      .Select(_ => _.ID)
      .First();
  }
}