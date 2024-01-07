namespace Centurion.Accounts.Core.Forms;

public class CheckBoxesGridField : FormField
{
  public IList<FormOptionValue> Rows { get; set; } = new List<FormOptionValue>();
  public IList<FormOptionValue> Columns { get; set; } = new List<FormOptionValue>();

  protected override bool IsValueValid(FormFieldValue fieldValue)
  {
    var ids = fieldValue.Value.Split(';', StringSplitOptions.RemoveEmptyEntries);

    return ids.All(id => Rows.Any(r => r.Id == id) || Columns.Any(c => c.Id == id));
  }
}