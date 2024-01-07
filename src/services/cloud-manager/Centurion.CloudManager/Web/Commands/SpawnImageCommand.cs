namespace Centurion.CloudManager.Web.Commands;

public class SpawnImageCommand
{
  public string ImageId { get; set; } = null!;
  public Dictionary<string, string> Parameters { get; set; } = new();
  public string? ServerId { get; set; }
}