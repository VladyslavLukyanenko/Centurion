namespace Centurion.Cli.Core.Domain.Fields;

public class TemplatedMultilineField : Field<string>, ISiblingsDependentField
{
  public TemplatedMultilineField()
  {
  }

  public TemplatedMultilineField(string? systemName = null, string? displayName = null, bool isRequired = true,
    Func<string?, Task<bool>>? validator = null)
    : base(systemName, displayName, isRequired, validator)
  {
  }


  public override string? ValueId => Value;
  public override string? DisplayValue => Value;

  public void Consume(IEnumerable<Field> siblings)
  {
    foreach (var sibling in siblings)
    {
      Value = Value?.Replace($"%{sibling.DisplayName}%", sibling.Value?.ToString());
    }
  }
}