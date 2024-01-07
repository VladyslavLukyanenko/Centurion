namespace Centurion.CloudManager.Web.Commands;

public class StartOrRunNodeCommand
{
  public string ProviderName { get; set; } = null!;
  public string? StoppedNodeId { get; set; }
}