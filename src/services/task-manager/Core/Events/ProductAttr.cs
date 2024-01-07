namespace Centurion.TaskManager.Core.Events;

public class ProductAttr
{
  public string Name { get; init; } = null!;
  public string Value { get; init; } = null!;
  public bool IsSensitive { get; init; }
}