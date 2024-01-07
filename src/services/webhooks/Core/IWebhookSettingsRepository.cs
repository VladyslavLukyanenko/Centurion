using Centurion.Contracts;

namespace Centurion.WebhookSender.Core;

public interface IWebhookSettingsRepository
{
  ValueTask<WebhookSettings?> GetAsync(string userId, CancellationToken ct = default);
  ValueTask CreateAsync(WebhookSettings settings, CancellationToken ct = default);
  void Update(WebhookSettings settings);
}