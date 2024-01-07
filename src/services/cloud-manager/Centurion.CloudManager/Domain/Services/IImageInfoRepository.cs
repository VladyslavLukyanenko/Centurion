using Centurion.SeedWork.Primitives;

namespace Centurion.CloudManager.Domain.Services;

public record struct ImageId(string Name, string? Ver);

public interface IImageInfoRepository : ICrudRepository<ImageInfo, string>
{
  ValueTask<IDictionary<string, ImageInfo>> GetLatestByNames(ISet<string> images,
    CancellationToken ct = default);
}