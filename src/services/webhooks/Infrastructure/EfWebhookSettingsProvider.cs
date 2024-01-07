using Centurion.Contracts;
using Centurion.WebhookSender.Core;
using Microsoft.EntityFrameworkCore;

namespace Centurion.WebhookSender.Infrastructure;

public class EfWebhookSettingsProvider : IWebhookSettingsProvider
{
  private readonly DbContext _context;

  public EfWebhookSettingsProvider(DbContext context)
  {
    _context = context;
  }

  public async ValueTask<WebhookSettings?> GetSettingsForUserAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Set<WebhookSettings>().AsNoTracking().FirstOrDefaultAsync(_ => _.UserId == userId, ct);
  }
}