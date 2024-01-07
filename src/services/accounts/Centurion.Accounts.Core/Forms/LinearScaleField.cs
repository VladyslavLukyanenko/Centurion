namespace Centurion.Accounts.Core.Forms;

public class LinearScaleField : FormField
{
  public byte Min { get; set; }
  public byte Max { get; set; }

  public string MinLabel { get; set; } = null!;
  public string MaxLabel { get; set; } = null!;

  protected override bool IsValueValid(FormFieldValue fieldValue) =>
    byte.TryParse(fieldValue.Value, out var value) && value >= Min && value <= Max;
}