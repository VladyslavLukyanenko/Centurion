namespace Centurion.Accounts.Core.Embeds.Services;

public interface IEmbedRenderer
{
  ValueTask<string> RenderAsync(DiscordEmbedWebHookBinding binding, IDictionary<string, object> context,
    CancellationToken ct = default);
}