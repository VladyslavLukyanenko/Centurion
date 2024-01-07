using Centurion.Cli.Core.Domain.Fields.Validation;

namespace Centurion.Cli.Core.Domain.Fields;

public class HiddenField : Field
{
  private readonly Func<object?, Task<bool>> _validator = FieldValidators.AlwaysValid<object?>();

  public HiddenField()
  {
  }

  public HiddenField(string systemName, object value)
    : base(systemName, null, false)
  {
    Value = value;
  }

  public override Task<bool> IsValid() => _validator(null);

  public override bool IsEmpty => false;

  public override string? ValueId => Value?.ToString();
  public override string? DisplayValue => ValueId;
}