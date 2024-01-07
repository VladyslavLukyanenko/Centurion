using Centurion.Accounts.Core.Products;
using NodaTime;

namespace Centurion.Accounts.Core.Forms;

public class Form : DashboardBoundEntity
{
  public Instant? PublishedAt { get; set; } 
  public FormSettings Settings { get; set; } = new();
  public FormThemeSettings Theme { get; set; } = new();
}