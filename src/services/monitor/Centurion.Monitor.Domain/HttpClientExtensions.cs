namespace Centurion.Monitor.Domain;

public static class HttpClientExtensions
{
  public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request,
    CancellationToken ct = default)
  {
    var clone = new HttpRequestMessage(request.Method, request.RequestUri)
    {
      Content = await request.Content.CloneAsync(ct).ConfigureAwait(false),
      Version = request.Version
    };

    foreach (KeyValuePair<string, object?> prop in request.Options)
    {
      clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
    }

    foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
    {
      clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
    }

    return clone;
  }

  public static async Task<HttpContent?> CloneAsync(this HttpContent? content, CancellationToken ct = default)
  {
    if (content == null)
    {
      return null;
    }

    var ms = new MemoryStream();
    await content.CopyToAsync(ms, ct).ConfigureAwait(false);
    ms.Position = 0;

    var clone = new StreamContent(ms);
    foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
    {
      clone.Headers.Add(header.Key, header.Value);
    }

    return clone;
  }
}