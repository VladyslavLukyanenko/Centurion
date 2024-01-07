namespace Centurion.Accounts.Core.Forms;

public class FormFieldValue
{
  private FormFieldValue()
  {
  }

  public FormFieldValue(string value, long fieldId)
  {
    Value = value;
    FieldId = fieldId;
  }

  public string Value { get; private set; } = null!;
  public long FieldId { get; private set; }
}