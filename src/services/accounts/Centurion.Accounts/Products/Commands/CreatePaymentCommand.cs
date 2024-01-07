using Newtonsoft.Json;

namespace Centurion.Accounts.Products.Commands;

public class CreatePaymentCommand
{
  public string Password { get; set; } = null!;
  [JsonProperty("g-captcha-response")] public string CaptchaResponse { get; set; } = null!;
}