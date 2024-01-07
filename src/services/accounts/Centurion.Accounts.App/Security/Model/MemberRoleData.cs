namespace Centurion.Accounts.App.Security.Model;

public class MemberRoleData
{
  public long Id { get; set; }
  public string Name { get; set; } = null!;
  public IList<string> Permissions { get; set; } = new List<string>();
  public decimal? Salary { get; set; }
  public int? Currency { get; set; }
  public int? PayoutFrequency { get; set; }
  public string? ColorHex { get; set; }
}