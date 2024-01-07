namespace Centurion.Cli.Core.Domain.Fields;

public class TextField : Field<string>
{
  public TextField()
  {
  }

  public TextField(string? systemName = null, string? displayName = null, bool isRequired = true,
    Func<string?, Task<bool>>? validator = null)
    : base(systemName, displayName, isRequired, validator)
  {
  }


  public override string? ValueId => Value;
  public override string? DisplayValue => Value;
}