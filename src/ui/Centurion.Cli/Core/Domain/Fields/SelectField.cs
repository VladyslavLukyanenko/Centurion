namespace Centurion.Cli.Core.Domain.Fields;

public class SelectField : Field<object>
{
  public SelectField()
  {
  }

  public SelectField(string? systemName = null, string? displayName = null, bool isRequired = true,
    Func<object?, Task<bool>>? validator = null, params KeyValuePair<string, object>[] options)
    : base(systemName, displayName, isRequired, validator)
  {
    Options = options.ToList().AsReadOnly();
  }

  public override string? ValueId => Value?.ToString();

  public override string? DisplayValue => Equals(Value, default)
    ? default
    : Options.Where(_ => Equals(_.Value, Value)).Select(_ => _.Key).FirstOrDefault();

  public IReadOnlyList<KeyValuePair<string, object>> Options { get; private set; } = null!;
}

public class SelectField<T> : SelectField
{
  public SelectField()
  {
  }

  public SelectField(string? systemName = null, string? displayName = null, bool isRequired = true,
    Func<object?, Task<bool>>? validator = null, params KeyValuePair<string, object>[] options)
    : base(systemName, displayName, isRequired, validator, options)
  {
  }

  public new T? Value
  {
    get => (T?) base.Value;
    set => base.Value = value;
  }
}