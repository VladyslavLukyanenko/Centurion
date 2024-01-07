using Centurion.Contracts;
using Centurion.WebhookSender.Core;
using Microsoft.EntityFrameworkCore;

namespace Centurion.WebhookSender.Infrastructure;

public class EfWebhookSettingsRepository : IWebhookSettingsRepository
{
  private readonly DbContext _context;

  public EfWebhookSettingsRepository(DbContext context)
  {
    _context = context;
  }

  public async ValueTask<WebhookSettings?> GetAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Set<WebhookSettings>().SingleOrDefaultAsync(_ => _.UserId == userId, ct);
  }

  public async ValueTask CreateAsync(WebhookSettings settings, CancellationToken ct = default)
  {
    await _context.AddAsync(settings, ct);
  }

  public void Update(WebhookSettings settings)
  {
    _context.Update(settings);
  }
}