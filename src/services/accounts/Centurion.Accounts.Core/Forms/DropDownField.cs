namespace Centurion.Accounts.Core.Forms;

public class DropDownField : FormField
{
  public IList<FormOptionValue> Options { get; set; } = new List<FormOptionValue>();

  protected override bool IsValueValid(FormFieldValue fieldValue) =>
    Options.Any(o => o.Id == fieldValue.Value);
}