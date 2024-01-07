using NodaTime.Text;

namespace Centurion.Accounts.Core.Forms;

public class TimeField : FormField
{
  protected override bool IsValueValid(FormFieldValue fieldValue) =>
    LocalTimePattern.GeneralIso.Parse(fieldValue.Value).Success;
}