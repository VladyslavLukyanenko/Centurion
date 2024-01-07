namespace Centurion.Cli.Core.Domain.Fields;

public abstract class RadioButtonGroupField : Field<object>
{
  public RadioButtonGroupField()
  {
  }

  protected RadioButtonGroupField(string systemName, Dictionary<string, object?> nameValues, string displayName = "",
    bool isRequired = false)
    : base(systemName, displayName, isRequired)
  {
    NameValues = nameValues;
    Value = nameValues.First().Value;
  }

  public Dictionary<string, object?> NameValues { get; } = null!;
}

public class RadioButtonGroupField<T> : RadioButtonGroupField
{
  private static readonly Func<T?, string?> FallbackIdFactory = v => v?.ToString();
  private readonly Func<T?, string?> _idFactory;

  public RadioButtonGroupField(string systemName, Dictionary<string, T> nameValues, string displayName = "",
    bool isRequired = false,
    Func<T?, string?>? idFactory = null)
    : base(systemName,
      new Dictionary<string, object?>(nameValues.Select(_ => new KeyValuePair<string, object?>(_.Key, _.Value))),
      displayName, isRequired)
  {
    _idFactory = idFactory ?? FallbackIdFactory;
  }

  public override string? ValueId => _idFactory(Value);

  public override string? DisplayValue => Equals(Value, default)
    ? default
    : NameValues.Where(_ => Equals(_.Value, Value)).Select(_ => _.Key).FirstOrDefault();

  public new T? Value
  {
    get => (T?) base.Value;
    set => base.Value = value;
  }
}