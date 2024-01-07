namespace Centurion.CloudManager.Web.Foundation.Config
{
  public class CorsConfig
  {
    public bool UseCors { get; set; }
    public List<string> AllowedHosts { get; set; } = new();
  }
}