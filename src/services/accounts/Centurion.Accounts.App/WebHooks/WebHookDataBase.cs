using Centurion.Accounts.App.Products.Model;

namespace Centurion.Accounts.App.WebHooks;

public abstract class WebHookDataBase
{
  public DashboardRef Dashboard { get; set; } = null!;
  public string Type => WebHookDataInspector.GetType(this);
}