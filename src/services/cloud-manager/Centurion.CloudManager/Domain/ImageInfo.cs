using Centurion.SeedWork.Primitives;
using NodaTime;

namespace Centurion.CloudManager.Domain;

public class ImageInfo : Entity<string>
{
  private HashSet<string> _requiredSpawnParameters = new();

  private ImageInfo()
  {
  }

  public ImageInfo(string id, string name, string version, string category, ISet<string> requiredSpawnParameters,
    Instant createdAt)
    : base(id.Trim())
  {
    Name = name.Trim();
    Version = version.Trim();
    Category = category;
    CreatedAt = createdAt;
    _requiredSpawnParameters = requiredSpawnParameters.Select(o => o.Trim()).ToHashSet();
  }

  public string Name { get; private set; } = null!;
  public string Version { get; private set; } = null!;
  public string Category { get; private set; } = null!;
  public Instant CreatedAt { get; private set; }

  public IReadOnlySet<string> RequiredSpawnParameters => _requiredSpawnParameters;

  public string GetFullName() => Name + ":" + Version;
}