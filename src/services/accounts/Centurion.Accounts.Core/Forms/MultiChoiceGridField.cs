namespace Centurion.Accounts.Core.Forms;

public class MultiChoiceGridField : FormField
{
  public IList<FormOptionValue> Rows { get; set; } = new List<FormOptionValue>();
  public IList<FormOptionValue> Columns { get; set; } = new List<FormOptionValue>();
  protected override bool IsValueValid(FormFieldValue fieldValue)
  {
    var ids = fieldValue.Value.Split(';', StringSplitOptions.RemoveEmptyEntries);
      
    return Rows.Any(r => ids.Contains(r.Id)) && Columns.Any(c => ids.Contains(c.Id));
  }
}