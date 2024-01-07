namespace Centurion.SeedWork.Web;

public class ServerBindingsConfig
{
  public int Http1Port { get; set; }
  public int? Http2Port { get; set; }
  public bool CleartextOn2Port { get; set; }
}