using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Audit;
using Centurion.Accounts.Core.Audit.Services;
using Centurion.Accounts.Core.Identity.Services;

namespace Centurion.Accounts.Foundation.Audit;

public class AuditMiddleware
{
  private readonly RequestDelegate _next;

  public AuditMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var identityProvider = context.RequestServices.GetRequiredService<IIdentityProvider>();
    var labelProvider = context.RequestServices.GetRequiredService<IChangeSetLabelProvider>();
    ChangeSet? changeSet = null;
    var label = labelProvider.GetLabel();
    var identity = identityProvider.GetCurrentIdentity();
    if (!string.IsNullOrEmpty(label) && identity.HasValue)
    {
      var mutableChangeSetProvider = context.RequestServices.GetRequiredService<IMutableChangeSetProvider>();

      changeSet = new ChangeSet(label, identity.Value);
      mutableChangeSetProvider.SetChangeSet(changeSet);
    }

    await _next(context);
    if (changeSet != null && !changeSet.IsEmpty())
    {
      var ctx = context.RequestServices.GetRequiredService<DbContext>();
      ctx.Add(changeSet);
      await ctx.SaveChangesAsync();
    }
  }
}