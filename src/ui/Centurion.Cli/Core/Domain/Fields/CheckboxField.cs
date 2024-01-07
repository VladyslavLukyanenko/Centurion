namespace Centurion.Cli.Core.Domain.Fields;

public class CheckboxField : Field<object>
{
  public CheckboxField()
  {
  }

  public CheckboxField(string? systemName, string? displayName, object? value = null, bool isRequired = false,
    Func<CheckboxField, Task<bool>>? validator = null)
    : base(systemName, displayName, isRequired)
  {
    Value = value ?? true;
    var func = validator ?? (v => Task.FromResult(!isRequired || IsChecked));
    Validator = v => func(this);
  }

  public override string ValueId => $"{SystemName}_{Value}";
  public override string DisplayValue => DisplayName ?? SystemName ?? ValueId;
  public bool IsChecked { get; set; }

  public override bool IsEmpty => !IsChecked;

  public override void CopyTo(Field field)
  {
    base.CopyTo(field);
    var checkbox = (CheckboxField) field;
    checkbox.IsChecked = IsChecked;
  }
}

public class CheckboxField<T> : CheckboxField
{
  public CheckboxField()
  {
  }

  public CheckboxField(T value, string? systemName = null, string? displayName = null, bool isRequired = false,
    Func<CheckboxField, Task<bool>>? validator = null)
    : base(systemName, displayName, value, isRequired, validator)
  {
  }

  public new T? Value
  {
    get => (T?) base.Value;
    set => base.Value = value;
  }
}