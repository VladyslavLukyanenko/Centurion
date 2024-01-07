namespace Centurion.Accounts.Core.Audit.Mappings;

public class AuditEntityMapping
{
  public AuditEntityMapping(PropertyValueAccessor idAccessor)
  {
    IdAccessor = idAccessor;
  }

  public IDictionary<string, PropertyValueAccessor> Accessors { get; } =
    new Dictionary<string, PropertyValueAccessor>();

  public IDictionary<string, Type> Converters { get; } = new Dictionary<string, Type>();

  public PropertyValueAccessor IdAccessor { get; private set; }
}