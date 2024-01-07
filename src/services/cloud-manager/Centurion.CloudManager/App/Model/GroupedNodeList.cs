using Centurion.CloudManager.Domain;

namespace Centurion.CloudManager.App.Model;

public class GroupedNodeList
{
  public string ProviderName { get; set; } = null!;
  public List<Node> Nodes { get; set; } = new();
}