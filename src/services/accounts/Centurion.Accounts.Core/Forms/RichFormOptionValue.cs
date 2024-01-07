namespace Centurion.Accounts.Core.Forms;

public class RichFormOptionValue
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public uint Order { get; set; }
  public RichFormFieldTitle Text { get; set; } = null!;
}