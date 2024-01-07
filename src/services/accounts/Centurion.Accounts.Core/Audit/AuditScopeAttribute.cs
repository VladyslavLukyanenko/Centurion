namespace Centurion.Accounts.Core.Audit;

[AttributeUsage(AttributeTargets.Method)]
public class AuditScopeAttribute
  : Attribute
{
  public AuditScopeAttribute(string scopeName)
  {
    ScopeName = scopeName;
  }

  public string ScopeName { get; }
}