using Docker.DotNet;
using Docker.DotNet.Models;
using Centurion.CloudManager.Domain;
using Centurion.CloudManager.Domain.Services;
using NodaTime.Extensions;

namespace Centurion.CloudManager.App.Services;

public class ImagesManager : IImagesManager
{
  private readonly IDockerClientsProvider _dockerClientsProvider;
  private readonly IImageInfoRepository _imageInfoRepository;
  private readonly IDockerAuthProvider _authProvider;
  private readonly ILogger<ImagesManager> _logger;

  public ImagesManager(IDockerClientsProvider dockerClientsProvider, IImageInfoRepository imageInfoRepository,
    IDockerAuthProvider authProvider, ILogger<ImagesManager> logger)
  {
    _dockerClientsProvider = dockerClientsProvider;
    _imageInfoRepository = imageInfoRepository;
    _authProvider = authProvider;
    _logger = logger;
  }

  public async Task<ImageInfo> AddImageAsync(string imageName, string? version, string category,
    ISet<string> spawnParams, CancellationToken ct = default)
  {
    var fullImgName = $"{imageName}:{version ?? "latest"}";
    var client = _dockerClientsProvider.GetLocalDockerClient();

    await client.Images.CreateImageAsync(new ImagesCreateParameters
      {
        FromImage = fullImgName
      },
      await _authProvider.GetAuthConfigAsync(ct),
      new Progress<JSONMessage>(),
      ct);

    var images = await client.Images.ListImagesAsync(new ImagesListParameters { All = true }, ct);
    var img = images.First(_ => _.RepoTags.Any(d => d == fullImgName));
    var imageInfo = new ImageInfo(img.ID, fullImgName, GetImageVersion(img.RepoTags), category, spawnParams,
      img.Created.ToInstant());
    await _imageInfoRepository.CreateAsync(imageInfo, ct);

    return imageInfo;
  }

  public async ValueTask<bool> TryStopContainersAsync(Node node, CancellationToken ct = default)
  {
    if (!node.IsReady)
    {
      return false;
    }

    var containerIds = node.Images.Select(_ => _.ContainerId).ToHashSet();
    node.ClearImages();
    var client = _dockerClientsProvider.GetClient(node);
    var allContainers = await client.Containers.ListContainersAsync(new ContainersListParameters
    {
      All = true
    }, ct);

    var containers = allContainers.Where(_ => containerIds.Contains(_.ID));

    foreach (var c in containers)
    {
      if (c.State == RuntimeImageInfo.RunningStateValue)
      {
        if (!await TryStopContainerByIdAsync(node, c.ID, ct))
        {
          return false;
        }
      }
      else
      {
        await RemoveContainerAndRefreshAsync(c.ID, ct, client);
      }
    }

    return true;
  }

  private static string GetImageVersion(IEnumerable<string> tags)
  {
    return tags.First(tag => !tag.EndsWith(":latest"));
  }

  public async Task<bool> TryStopContainerAsync(Node node, string imageId, string? containerId,
    CancellationToken ct = default)
  {
    if (!node.IsReady)
    {
      return false;
    }

    var client = _dockerClientsProvider.GetClient(node);
    var allContainers = await client.Containers.ListContainersAsync(new ContainersListParameters
    {
      All = true
    }, ct);

    var containers = allContainers.Where(_ => _.ImageID == imageId);
    if (!string.IsNullOrEmpty(containerId))
    {
      containers = containers.Where(c => c.ID == containerId);
    }

    foreach (var c in containers)
    {
      if (c.State == RuntimeImageInfo.RunningStateValue)
      {
        if (!await TryStopContainerByIdAsync(node, c.ID, ct))
        {
          return false;
        }
      }
      else
      {
        await RemoveContainerAndRefreshAsync(c.ID, ct, client);
      }
    }

    return true;
  }


  public async Task SpawnImageAsync(ImageInfo image, Node node, IDictionary<string, string> spawnParams,
    IEnumerable<PortBindingsConfig>? portBindings = null, CancellationToken ct = default)
  {
    if (image == null)
    {
      throw new ArgumentNullException(nameof(image));
    }

    if (node == null)
    {
      throw new ArgumentNullException(nameof(node));
    }

    if (spawnParams == null)
    {
      throw new ArgumentNullException(nameof(spawnParams));
    }

    var client = _dockerClientsProvider.GetClient(node);
    var existingImages = await client.Images.ListImagesAsync(new ImagesListParameters
    {
      All = true
    }, ct);

    var imgNameVal = image.GetFullName();
    if (!existingImages.Any(img => img.RepoTags.Contains(imgNameVal)))
    {
      await CreateImage(image, client, ct);
    }

    var containerParams = CreateContainerStartParams(image, spawnParams, portBindings);
    var r = await client.Containers.CreateContainerAsync(containerParams, ct);
    var started = await client.Containers.StartContainerAsync(r.ID, new ContainerStartParameters(), ct);
    if (!started)
    {
      throw new InvalidOperationException("Unable to start container for image " + image.Id);
    }

    node.SetImages(new[]
    {
      RuntimeImageInfo.CreatePending(node, image, r.ID, spawnParams)
    });
  }

  private static CreateContainerParameters CreateContainerStartParams(ImageInfo image,
    IDictionary<string, string> spawnParams,
    IEnumerable<PortBindingsConfig>? portBindings)
  {
    var list = spawnParams.Select(p =>
      {
        if (string.IsNullOrEmpty(p.Value))
        {
          return p.Key;
        }

        return $"{p.Key}={p.Value}";
      })
      .ToList();

    var envLabels = spawnParams.ToDictionary(_ => RuntimeImageInfo.MaskEnvForLabel(_.Key), _ => _.Value);

    var containerParams = new CreateContainerParameters
    {
      Image = image.Name,
      Env = list,
      Labels = envLabels,
      HostConfig = new HostConfig
      {
        RestartPolicy = new RestartPolicy
        {
          Name = RestartPolicyKind.Always
        },
      }
    };

    if (portBindings is not null)
    {
      foreach (var portBinding in portBindings)
      {
        containerParams.HostConfig.PortBindings = new Dictionary<string, IList<PortBinding>>
        {
          [portBinding.Container + "/tcp"] = new List<PortBinding>
          {
            new()
            {
              HostPort = portBinding.Host.ToString()
            }
          }
        };
      }
    }

    return containerParams;
  }

  private async Task CreateImage(ImageInfo image, DockerClient client, CancellationToken ct)
  {
    var authConfig = await _authProvider.GetAuthConfigAsync(ct);
    var progress = new Progress<JSONMessage>(m =>
    {
      if (string.IsNullOrEmpty(m.ErrorMessage))
      {
        _logger.LogDebug("Image create progress: {Message}", m.ProgressMessage);
      }
      else
      {
        _logger.LogError("Image create error: {ErrorMessage}", m.ErrorMessage);
      }
    });
    await client.Images.CreateImageAsync(new ImagesCreateParameters
      {
        FromImage = image.GetFullName()
      },
      authConfig,
      progress,
      ct);
  }

  private async Task<bool> TryStopContainerByIdAsync(Node node, string containerId,
    CancellationToken ct = default)
  {
    var client = _dockerClientsProvider.GetClient(node);
    var stopResult = await client.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), ct);
    if (!stopResult)
    {
      return false;
    }

    return await RemoveContainerAndRefreshAsync(containerId, ct, client);
  }

  private async Task<bool> RemoveContainerAndRefreshAsync(string containerId, CancellationToken ct,
    DockerClient client)
  {
    await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters
    {
      RemoveVolumes = false,
      Force = true
    }, ct);

    return true;
  }
}