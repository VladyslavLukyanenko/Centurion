using System.Net.Http.Headers;
using Centurion.Cli.Core.Services;

namespace Centurion.Cli.Core.Clients;

public class BearerTokenHandler : DelegatingHandler
{
  private readonly ITokenProvider _token;

  public BearerTokenHandler(ITokenProvider token)
  {
    _token = token;
  }

  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
  {
    if (!request.Headers.Contains("Authorization") && !string.IsNullOrEmpty(_token.CurrentAccessToken))
    {
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token.CurrentAccessToken);
    }

    return await base.SendAsync(request, ct);
  }
}