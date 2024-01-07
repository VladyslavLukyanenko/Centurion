namespace Centurion.CloudManager.Web.Commands;

public class CreateImageCommand
{
  public string ImageName { get; init; } = null!;
  public string? ImageVersion { get; init; } = null!;
  public string Category { get; init; } = null!;
  public HashSet<string> RequiredSpawnParameters { get; set; } = new();
}