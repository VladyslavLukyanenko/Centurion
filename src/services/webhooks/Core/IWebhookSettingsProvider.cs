using Centurion.Contracts;

namespace Centurion.WebhookSender.Core;

public interface IWebhookSettingsProvider
{
  ValueTask<WebhookSettings?> GetSettingsForUserAsync(string userId, CancellationToken ct = default);
}