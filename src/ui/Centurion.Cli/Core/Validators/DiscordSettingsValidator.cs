using Centurion.Contracts;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace Centurion.Cli.Core.Validators;

public class DiscordSettingsValidator : AbstractValidator<WebhookSettings>
{
  const string NotAvailableMessage = "Provided webhook url is not available or invalid. Please check it again.";
  const string InvalidUrlMessage = "Invalid discord webhook url provided.";

  public DiscordSettingsValidator()
  {
    RuleFor(_ => _.SuccessUrl)
      .Must(BeValidUrl)
      .WithMessage(InvalidUrlMessage)
      .MustAsync(BeValidHook)
      .WithMessage(NotAvailableMessage);

    RuleFor(_ => _.FailureUrl)
      .Must(BeValidUrl)
      .WithMessage(InvalidUrlMessage)
      .MustAsync(BeValidHook)
      .WithMessage(NotAvailableMessage);
  }

  private bool BeValidUrl(WebhookSettings settings, string? url)
  {
    return !string.IsNullOrEmpty(url) && url.Contains("https://discord", StringComparison.InvariantCultureIgnoreCase);
  }

  private async Task<bool> BeValidHook(WebhookSettings settings, string? rawUrl, CancellationToken ct)
  {
    if (string.IsNullOrEmpty(rawUrl) || !BeValidUrl(settings, rawUrl)
                                     || !Uri.TryCreate(rawUrl, UriKind.RelativeOrAbsolute, out var url))
    {
      return false;
    }

    var client = new HttpClient();
    var resp = await client.GetAsync(url, ct);
    if (!resp.IsSuccessStatusCode)
    {
      return false;
    }

    var json = await resp.Content.ReadAsStringAsync(ct);
    var obj = JObject.Parse(json);

    return obj.ContainsKey("name");
  }
}