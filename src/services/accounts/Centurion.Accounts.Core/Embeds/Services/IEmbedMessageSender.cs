using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Embeds.Services;

public interface IEmbedMessageSender
{
  ValueTask<Result> SendAsync(string serializedPayload, DiscordEmbedWebHookBinding binding, CancellationToken ct = default);
}