using Microsoft.AspNetCore.Mvc;

namespace Centurion.Accounts.Foundation.Mvc;

public class ApiHttpGetV1Attribute : HttpGetAttribute
{
  public ApiHttpGetV1Attribute(string template = "", bool fromRoot = false)
    : base(fromRoot && !template.StartsWith("/")
      ? "/" + ApiV1HttpRouteAttribute.Prefix + template
      : ApiV1HttpRouteAttribute.Prefix + template)
  {
    FromRoot = fromRoot;
  }

  public bool FromRoot { get; }
}