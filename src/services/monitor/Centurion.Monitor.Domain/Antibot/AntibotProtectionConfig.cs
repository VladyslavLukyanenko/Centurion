namespace Centurion.Monitor.Domain.Antibot;

public class AntibotProtectionConfig
{
  public TimeSpan CookieLifetime { get; init; }
  public string ProtectProvider { get; init; } = null!;
  public string ApiKey { get; init; } = null!;
  public IDictionary<string, string> AdditionalConfig { get; set; } = new Dictionary<string, string>();

  public bool IsEmpty() => string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(ProtectProvider);

  public override string ToString()
  {
    if (IsEmpty())
    {
      return "<Empty>";
    }

    return $"{ProtectProvider}, Key={ApiKey}, Lifetime={CookieLifetime.ToString()}";
  }
}