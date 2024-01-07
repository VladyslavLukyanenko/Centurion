namespace Centurion.Accounts.App.Security.Model;

public class BoundMemberRoleData
{
  public long RoleBindingId { get; set; }
  public string RoleName { get; set; } = null!;
  public IEnumerable<string> Permissions { get; set; } = new List<string>();
  public long MemberRoleId { get; set; }
  public string? ColorHex { get; set; }
}