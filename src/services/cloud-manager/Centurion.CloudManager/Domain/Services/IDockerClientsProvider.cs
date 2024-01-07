using System.Diagnostics.CodeAnalysis;
using Docker.DotNet;

namespace Centurion.CloudManager.Domain.Services;

public interface IDockerClientsProvider
{
  DockerClient GetLocalDockerClient();
  void AddClient(Node node);
  void RemoveClient(Node node);
  DockerClient GetClient(Node node);
  bool TryGetClient(Node node, [MaybeNullWhen(false)] out DockerClient client);
}