namespace Centurion.Accounts.Core.Forms;

public class TextBlockField : FormField
{
  protected override bool IsValueValid(FormFieldValue fieldValue) => false; // this field can't have any value
}