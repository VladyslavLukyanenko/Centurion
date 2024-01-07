using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using Docker.DotNet;

namespace Centurion.CloudManager.Infra.Services;

public class DockerClientsProvider : IDockerClientsProvider
{
  private static readonly TimeSpan ClientTimeout = TimeSpan.FromSeconds(5);
  private readonly ConcurrentDictionary<string, Lazy<DockerClient>> _clientsCache = new();

  public DockerClient GetLocalDockerClient()
  {
    return new DockerClientConfiguration(new Uri("http://localhost:2375")).CreateClient();
  }

  public void AddClient(Node node)
  {
    _clientsCache.GetOrAdd(node.Id, _ => new Lazy<DockerClient>(() =>
      new DockerClientConfiguration(node.DockerRemoteApiUrl, defaultTimeout: ClientTimeout)
        .CreateClient()));
  }

  public void RemoveClient(Node node)
  {
    _clientsCache.Remove(node.Id, out _);
  }

  public DockerClient GetClient(Node node)
  {
    if (!TryGetClient(node, out var client))
    {
      throw new InvalidOperationException($"Client for '{node.DockerRemoteApiUrl}' not connected.");
    }

    return client;
  }

  public bool TryGetClient(Node node, [MaybeNullWhen(false)] out DockerClient client)
  {
    client = null;
    if (_clientsCache.TryGetValue(node.Id, out var entry))
    {
      client = entry.Value;
      return true;
    }

    return false;
  }
}