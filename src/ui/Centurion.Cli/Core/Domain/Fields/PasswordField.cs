namespace Centurion.Cli.Core.Domain.Fields;

public class PasswordField : Field<string>
{
  public PasswordField()
  {
  }

  public PasswordField(string systemName, string displayName, bool isRequired)
    : base(systemName, displayName, isRequired)
  {
  }

  public override string? ValueId => Value;
  public override string? DisplayValue => Value;
}