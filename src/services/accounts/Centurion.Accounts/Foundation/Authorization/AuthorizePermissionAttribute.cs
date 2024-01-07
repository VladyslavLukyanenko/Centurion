using Microsoft.AspNetCore.Authorization;

namespace Centurion.Accounts.Foundation.Authorization;

public class AuthorizePermissionAttribute : AuthorizeAttribute
{
  public AuthorizePermissionAttribute(string permission)
  {
    Permission = permission;
  }

  public string Permission { get; }
}