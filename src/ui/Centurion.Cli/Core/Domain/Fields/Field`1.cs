using Centurion.Cli.Core.Domain.Fields.Validation;

namespace Centurion.Cli.Core.Domain.Fields;

public abstract class Field<T> : Field
{
  protected Func<T?, Task<bool>> Validator;

  public Field()
  {
    Validator = FieldValidators.NonDefaultValue<T?>();
  }

  protected Field(string? systemName, string? displayName, bool isRequired, Func<T?, Task<bool>>? validator = null)
    : base(systemName, displayName, isRequired)
  {
    Validator = validator ?? (isRequired ? FieldValidators.NonDefaultValue<T?>() : FieldValidators.AlwaysValid<T?>());
  }

  public static implicit operator T?(Field<T> src) => src.Value;

  public override Task<bool> IsValid() => Validator.Invoke(Value);

  public new virtual T? Value
  {
    get => base.Value == default ? default : (T) base.Value;
    set => base.Value = value;
  }

  public override void CopyTo(Field field)
  {
    base.CopyTo(field);
    var f = (Field<T>) field;
    f.Validator = Validator;
  }
}