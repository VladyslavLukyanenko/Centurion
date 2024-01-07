namespace Centurion.Cli.Core.Services;

public interface IWebHookManager
{
  // void EnqueueWebhook(PendingRaffleTask task);
  Task<bool> TestWebhook(string webhookUrl);
}