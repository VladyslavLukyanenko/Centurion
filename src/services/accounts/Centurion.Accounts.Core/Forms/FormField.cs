namespace Centurion.Accounts.Core.Forms;

public abstract class FormField : FormComponent
{
  public bool IsRequired { get; set; }
  public long SectionId { get; set; }

  public bool IsValid(FormFieldValue? fieldValue)
  {
    return !IsRequired && fieldValue == null
           || fieldValue != null && !string.IsNullOrEmpty(fieldValue.Value) && IsValueValid(fieldValue);
  }

  protected abstract bool IsValueValid(FormFieldValue fieldValue);
}