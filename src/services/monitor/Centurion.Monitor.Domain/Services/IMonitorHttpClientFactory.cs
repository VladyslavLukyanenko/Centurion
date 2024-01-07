using System.Net;

namespace Centurion.Monitor.Domain.Services;

public interface IMonitorHttpClientFactory
{
  HttpClient CreateHttpClient();
  bool UseCookies { get; set; }
  CookieContainer CookieContainer { get; set; }
  Uri? BaseAddress { get; set; }
}