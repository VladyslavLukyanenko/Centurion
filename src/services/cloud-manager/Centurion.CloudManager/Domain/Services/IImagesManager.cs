using Centurion.CloudManager.App;

namespace Centurion.CloudManager.Domain.Services;

public interface IImagesManager
{
  Task<ImageInfo> AddImageAsync(string imageName, string? version, string category, ISet<string> spawnParams,
    CancellationToken ct = default);

  ValueTask<bool> TryStopContainersAsync(Node node, CancellationToken ct = default);

  Task<bool> TryStopContainerAsync(Node node, RuntimeImageInfo image, CancellationToken ct = default)
  {
    return TryStopContainerAsync(node, image.ImageId, null, ct);
  }

  Task<bool> TryStopContainerAsync(Node node, string imageId, string? containerId,
    CancellationToken ct = default);

  async Task<bool> TryStopContainerAsync(IEnumerable<Node> nodes, ImageInfo image,
    CancellationToken ct = default)
  {
    var areAllStopped = true;
    foreach (var node in nodes)
    {
      var runtimeImages = node.Images.Where(_ => _.ImageId == image.Id);
      foreach (var runtimeImage in runtimeImages)
      {
        areAllStopped = await TryStopContainerAsync(node, image.Id, runtimeImage.ContainerId, ct) && areAllStopped;
      }
    }

    return areAllStopped;
  }

  Task SpawnImageAsync(ImageInfo image, Node server, IDictionary<string, string> spawnParams,
    IEnumerable<PortBindingsConfig>? portBindings = null, CancellationToken ct = default);
}