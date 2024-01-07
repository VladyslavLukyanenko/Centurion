using Centurion.Contracts;
using CSharpFunctionalExtensions;

namespace Centurion.Cli.Core.Services;

public interface IDiscordSettingsService : IAppStateHolder
{
  IObservable<WebhookSettings> Settings { get; }
  ValueTask<Result> UpdateAsync(WebhookSettings settings, CancellationToken ct = default);
}