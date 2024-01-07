using Docker.DotNet;
using Docker.DotNet.Models;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using NodeStatus = Centurion.CloudManager.Domain.NodeStatus;

namespace Centurion.CloudManager.App.Services;

public class ImagesRuntimeInfoService : IImagesRuntimeInfoService
{
  private readonly IDockerClientsProvider _dockerClientsProvider;

  private readonly IImageInfoRepository _imageInfoRepository;
  private readonly IMemoryCache _cache;

  private readonly ILogger<ImagesRuntimeInfoService> _logger;

  public ImagesRuntimeInfoService(IDockerClientsProvider dockerClientsProvider,
    IImageInfoRepository imageInfoRepository, ILogger<ImagesRuntimeInfoService> logger, IMemoryCache cache)
  {
    _dockerClientsProvider = dockerClientsProvider;
    _imageInfoRepository = imageInfoRepository;
    _logger = logger;
    _cache = cache;
  }

  public async Task RefreshStateAsync(IEnumerable<Node> nodes, CancellationToken ct = default)
  {
    _logger.LogDebug("Refreshing state");
    var imgInfoes = await _cache.GetOrCreateAsync("AllImages", async e =>
    {
      e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
      return await _imageInfoRepository.ListAllAsync(ct);
    });
    var clients = new Dictionary<Node, DockerClient>();
    foreach (var node in nodes)
    {
      _logger.LogDebug("Fetching docker client for node {NodeId}", node.Id);
      if (node.Status is not NodeStatus.Running)
      {
        node.Checked(false);
        continue;
      }

      if (!_dockerClientsProvider.TryGetClient(node, out var client))
      {
        _dockerClientsProvider.AddClient(node);
        client = _dockerClientsProvider.GetClient(node);
      }

      _logger.LogDebug("Docker client found {IsFound} for node {NodeId}", (client != null).ToString(),
        node.Id);
      clients[node] = client!;
    }

    _logger.LogDebug("Fetching running docker containers");
    var tasks = clients.Select(async p =>
    {
      var (node, client) = p;

      var isAvailable = node.DockerRemoteApiUrl != null && await IsRemoteApiAvailableAsync(client, ct);
      _logger.LogDebug("Fetching running containers for node {NodeId}, is available {IsAvailable}", node.Id,
        isAvailable.ToString());
      node.Checked(isAvailable);
      if (!node.IsReady)
      {
        return;
      }

      var containers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true }, ct);
      var images = imgInfoes.SelectMany(m => containers.Where(c => c.Image == m.Name)
          .Select(c =>
          {
            var envVars = c.Labels.Where(l => RuntimeImageInfo.IsMaskedEnv(l.Key))
              .Select(l => KeyValuePair.Create(RuntimeImageInfo.UnmaskEnv(l.Key), l.Value));
            return new RuntimeImageInfo(node, m, c.ID, c.Status, c.State, c.Created, envVars);
          }))
        .ToList();

      _logger.LogDebug("Running containers on node {NodeId} {@ImageInfos}", node.Id, images.Select(_ => _.Name));
      node.SetImages(images);
    });

    await Task.WhenAll(tasks);
    _logger.LogDebug("Fetched");
  }

  private async Task<bool> IsRemoteApiAvailableAsync(DockerClient client, CancellationToken ct = default)
  {
    try
    {
      var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
      cts.CancelAfter(TimeSpan.FromSeconds(2));
      await client.System.PingAsync(cts.Token);
      return true;
    }
    catch
    {
      return false;
    }
  }
}