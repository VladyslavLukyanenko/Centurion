namespace Centurion.Cli.Core.Domain.Fields;

public class OptionsField : Field<string>
{
  private bool _pickRandom;
  private readonly Random _random = new Random((int) DateTime.Now.Ticks);
  private string? _selectedValue;

  public OptionsField()
  {
  }

  public OptionsField(string systemName, string displayName, bool isRequired,
    IEnumerable<KeyValuePair<string, string>> options, Func<object?, Task<bool>>? validator = null)
    : base(systemName, displayName, isRequired, validator)
  {
    Options = options.ToList().AsReadOnly();
    if (Options.Count == 0)
    {
      throw new InvalidOperationException("Options can't be empty");
    }
  }

  public OptionsField(string systemName, string displayName, bool isRequired, IEnumerable<string> options,
    Func<object?, Task<bool>>? validator = null)
    : this(systemName, displayName, isRequired, options.Select(o => new KeyValuePair<string, string>(o, o)),
      validator)
  {
  }

  public bool PickRandom
  {
    get => _pickRandom;
    set
    {
      _pickRandom = value;
      Value = value ? Options[_random.Next(0, Options.Count)].Value : SelectedValue;
    }
  }

  public override string? ValueId => Value;

  public override string? DisplayValue => Equals(Value, default)
    ? default
    : Options.Where(_ => Equals(_.Value, Value)).Select(_ => _.Key).FirstOrDefault();

  public string? SelectedValue
  {
    get => _selectedValue;
    set
    {
      _selectedValue = value;
      if (PickRandom)
      {
        return;
      }

      Value = value;
    }
  }

  public IReadOnlyList<KeyValuePair<string, string>> Options { get; private set; } = null!;

  public override void CopyTo(Field field)
  {
    var o = (OptionsField) field;
    o._selectedValue = _selectedValue;
    o.Options = Options.ToList();
    base.CopyTo(field);
    o.PickRandom = PickRandom;
  }
}