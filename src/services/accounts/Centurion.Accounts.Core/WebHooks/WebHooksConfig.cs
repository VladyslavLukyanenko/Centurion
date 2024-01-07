using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.WebHooks;

public class WebHooksConfig : Entity
{
  public Guid DashboardId { get; set; }
  public bool IsEnabled { get; set; }
  public string ClientSecret { get; set; } = null!;
}