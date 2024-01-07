namespace Centurion.Accounts.Core.Products;

public class StripeIntegrationConfig
{
  public StripeIntegrationConfig()
  {
  }

  public StripeIntegrationConfig(string apiKey, string webHookEndpointSecret)
  {
    ApiKey = apiKey;
    WebHookEndpointSecret = webHookEndpointSecret;
  }

  public string ApiKey { get; set; } = null!;
  public string WebHookEndpointSecret { get; set; } = null!;
}