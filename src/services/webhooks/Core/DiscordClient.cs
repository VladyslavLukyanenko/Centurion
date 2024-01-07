using System.Diagnostics;
using System.Net;
using System.Text;
using Centurion.Contracts.Checkout.Integration;
using Centurion.Monitor.Domain.Services;
using Centurion.TaskManager;
using Centurion.WebhookSender.Core.Discord;
using CSharpFunctionalExtensions;
using Elastic.Apm.Api;

namespace Centurion.WebhookSender.Core;

public class DiscordClient : IDiscordClient
{
  private readonly HttpClient _client;
  private readonly IWebhookSettingsProvider _settingsProvider;
  private readonly ILogger<DiscordClient> _logger;
  private readonly IJsonSerializer _jsonSerializer;
  private readonly ITracer _tracer;

  public DiscordClient(HttpClient client, IWebhookSettingsProvider settingsProvider, ILogger<DiscordClient> logger,
    IJsonSerializer jsonSerializer, ITracer tracer)
  {
    _client = client;
    _settingsProvider = settingsProvider;
    _logger = logger;
    _jsonSerializer = jsonSerializer;
    _tracer = tracer;
  }

  public async ValueTask<Result> SendNotificationAsync(ProductCheckedOut @event, CancellationToken ct = default)
  {
    var settings = await _settingsProvider.GetSettingsForUserAsync(@event.UserId, ct);
    if (settings == null)
    {
      return Result.Failure("Webhook settings not found");
    }

    while (true)
    {
      var submitSpan = _tracer.StartSpan("submit_webhook", TraceConsts.Activities.DiscordClient);
      try
      {
        var formattedMessage = FormatMessage(@event);

        var webhookPayload = await _jsonSerializer.SerializeAsync(formattedMessage, ct);
        var message = new HttpRequestMessage(HttpMethod.Post, settings.SuccessUrl)
        {
          Content = new StringContent(webhookPayload, Encoding.UTF8, "application/json")
        };

        var response = await _client.SendAsync(message, HttpCompletionOption.ResponseContentRead, ct);
        if (submitSpan?.Context != null)
        {
          submitSpan.Context.Http = new Http
          {
            Url = message.RequestUri?.ToString(),
            Method = message.Method.Method,
            StatusCode = (int)response.StatusCode
          };
        }

        if (response.IsSuccessStatusCode)
        {
          return Result.Success();
        }

        if (await ProcessFailure(response, ct))
        {
          continue;
        }

        return Result.Failure("failed to submit webhook");
      }
      finally
      {
        submitSpan?.End();
      }
    }
  }

  private async Task<bool> ProcessFailure(HttpResponseMessage response, CancellationToken ct)
  {
    var submitSpan = _tracer.CurrentSpan;
    var responseContent = await response.Content.ReadAsStringAsync(ct);
    submitSpan?.SetLabel("response", responseContent);
    switch (response.StatusCode)
    {
      case HttpStatusCode.TooManyRequests:
      {
        var err = await _jsonSerializer.DeserializeAsync<DiscordRateLimitedError>(responseContent, ct);

        var delay = TimeSpan.FromMilliseconds(err!.RetryAfter);
        submitSpan?.SetLabel("rate_limited", delay.ToString());
        var delaySpan =
          _tracer.CurrentTransaction?.StartSpan("rate_limit_delay", TraceConsts.Activities.DiscordClient);
        _logger.LogWarning("Rate limited for {Delay}", delay);
        delaySpan?.SetLabel("rate_limited_for", delay.ToString());

        await Task.Delay(delay, ct);
        delaySpan?.End();
        return true;
      }
      case HttpStatusCode.BadRequest:
        submitSpan?.CaptureError("Failed to send webhook.", "bad_request", Array.Empty<StackFrame>());
        _logger.LogError("Failed to send webhook notifications. Response: {DiscordResponse}", responseContent);
        break;

      case HttpStatusCode.NotFound:
      case HttpStatusCode.Unauthorized:
        submitSpan?.CaptureError("Invalid webhook url.", "invalid_url", Array.Empty<StackFrame>());
        _logger.LogError(
          "Invalid webhook url. Discord respond with {StatusCode}. Probably it was removed or typo in url",
          response.StatusCode);
        break;
    }

    return false;
  }

  private DiscordWebhookBody FormatMessage(ProductCheckedOut n)
  {
    var embed = new Embed
    {
      Title = "Successful Checkout",
      Timestamp = n.Meta.Timestamp.ToDateTimeOffset()
    };

    var body = new DiscordWebhookBody
    {
      Username = "Centurion",
      Embeds = { embed }
    };

    if (!string.IsNullOrEmpty(n.Picture))
    {
      body.AvatarUrl = n.Picture;
      embed.Thumbnail.Url = n.Picture;
    }

    if (!string.IsNullOrEmpty(n.ShopIconUrl))
    {
      embed.Author ??= new Author();
      body.AvatarUrl = n.ShopIconUrl;
      embed.Author.IconUrl = n.ShopIconUrl;
    }

    if (!string.IsNullOrEmpty(n.ShopTitle))
    {
      embed.Author ??= new Author();
      embed.Author.Name = n.ShopTitle;
    }

    if (n.Url != null)
    {
      embed.Url = n.Url;
    }

    embed.Fields.Add(new Field
    {
      Name = "Product",
      Value = n.Title
    });

    embed.Fields.Add(new Field
    {
      Name = "SKU",
      Value = n.Sku
    });

    embed.Fields.Add(new Field
    {
      Name = "Site",
      Value = n.Store
    });

    // embed.Fields.Add(new Field
    // {
    //   Name = "Mode",
    //   Value = n.Mode
    // });

    embed.Fields.Add(new Field
    {
      Name = "Profile",
      Value = "||" + n.Profile + "||"
    });

    if (!string.IsNullOrEmpty(n.Proxy))
    {
      embed.Fields.Add(new Field
      {
        Name = "Proxy",
        Value = "||" + n.Proxy + "||"
      });
    }

    if (!string.IsNullOrEmpty(n.FormattedPrice))
    {
      embed.Fields.Add(new Field
      {
        Name = "Price",
        Value = n.FormattedPrice
      });
    }

    if (!string.IsNullOrEmpty(n.Account))
    {
      embed.Fields.Add(new Field
      {
        Name = "Account",
        Value = "||" + n.Account + "||"
      });
    }
    //
    // embed.Fields.Add(new Field
    // {
    //   Name = "Checkout Speed",
    //   Value = n.Duration.ToTimeSpan().ToFriendlyString()
    // });

    foreach (var attr in n.Attrs.Where(a => !string.IsNullOrEmpty(a.Value)))
    {
      embed.Fields.Add(new Field { Name = attr.Name, Value = attr.Value });
    }

    foreach (var link in n.Links.Where(a => !string.IsNullOrEmpty(a.Url)))
    {
      embed.Fields.Add(new Field { Name = link.Name, Value = $"[Link]({link.Url})" });
    }

    return body;
  }
}