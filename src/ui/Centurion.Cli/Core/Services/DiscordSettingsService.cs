using System.Reactive.Subjects;
using Centurion.Cli.Core.Services.Modules;
using Centurion.Contracts;
using Centurion.Contracts.Webhooks;
using CSharpFunctionalExtensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Centurion.Cli.Core.Services;

public class DiscordSettingsService : ExecutionStatusProviderBase, IDiscordSettingsService
{
  private readonly BehaviorSubject<WebhookSettings> _settings = new(new WebhookSettings());
  private readonly Webhooks.WebhooksClient _client;

  public DiscordSettingsService(Webhooks.WebhooksClient client)
  {
    _client = client;
    Settings = _settings;
  }

  public IObservable<WebhookSettings> Settings { get; }

  public ValueTask<Result> InitializeAsync(CancellationToken ct = default) => Guard.ExecuteSafe(async () =>
  {
    WebhookSettings? item = null;
    try
    {
      item = await _client.GetSettingsAsync(new Empty(), cancellationToken: ct).TrackProgress(FetchingTracker);
    }
    catch (RpcException)
    {
    }

    _settings.OnNext(item ?? new WebhookSettings());
  });

  public void ResetCache()
  {
    _settings.OnNext(new WebhookSettings());
  }

  public ValueTask<Result> UpdateAsync(WebhookSettings settings, CancellationToken ct = default) =>
    Guard.ExecuteSafe(async () =>
      {
        await _client.SaveSettingsAsync(new SaveSettingsCommand
        {
          SuccessUrl = settings.SuccessUrl,
          FailureUrl = settings.FailureUrl
        }, cancellationToken: ct);
        _settings.OnNext(settings);
      })
      .TrackProgress(FetchingTracker);
}