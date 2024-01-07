namespace Centurion.Accounts.Core.Forms;

public class FormOptionValue
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public uint Order { get; set; }
  public string Text { get; set; } = null!;
}