using Centurion.Contracts;

namespace Centurion.Monitor;

public static class ProxyDataExtensions
{
  public static Uri ToUri(this ProxyData p) => new(p.Value);
}