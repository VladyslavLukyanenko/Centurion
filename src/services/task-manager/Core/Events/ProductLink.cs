namespace Centurion.TaskManager.Core.Events;

public class ProductLink
{
  public string Name { get; init; } = null!;
  public string Url { get; init; } = null!;
  public bool IsSensitive { get; init; }
}