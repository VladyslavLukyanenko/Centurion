using Centurion.CloudManager.App;
using Centurion.CloudManager.Domain;

namespace Centurion.CloudManager.Web.Services;

public class CheckoutServiceSpawnConfig
{
  private static readonly List<Func<UserInfo, string, string>> Renderers = new()
  {
    (info, value) => value.Replace("{UserId}", info.Id),
    (info, value) => value.Replace("{UserName}", info.Name),
    (info, value) => value.Replace("{Date}", DateTimeOffset.UtcNow.ToString("yyyyMMdd")),
  };

  public string ImageName { get; set; } = null!;
  public Dictionary<string, string> Env { get; set; } = null!;
  public PortBindingsConfig PortBinding { get; set; } = null!;
  public string Schema { get; set; } = null!;

  public IDictionary<string, string> RenderEnv(UserInfo userInfo) =>
    Env.ToDictionary(_ => _.Key, _ => Renderers.Aggregate(_.Value, (curr, renderer) => renderer(userInfo, curr)));

  public Uri GetAbsoluteUrl(Node node) => new UriBuilder(Schema, node.PublicDnsName, PortBinding.Host).Uri;
}