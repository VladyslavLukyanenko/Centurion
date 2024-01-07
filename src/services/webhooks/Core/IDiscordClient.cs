using Centurion.Contracts.Checkout.Integration;
using CSharpFunctionalExtensions;

namespace Centurion.WebhookSender.Core;

public interface IDiscordClient
{
  ValueTask<Result> SendNotificationAsync(ProductCheckedOut @event, CancellationToken ct = default);
}