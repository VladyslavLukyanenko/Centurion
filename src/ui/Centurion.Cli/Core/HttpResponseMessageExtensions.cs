using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;

namespace Centurion.Cli.Core;

public static class HttpResponseMessageExtensions
{
  [DoesNotReturn]
  public static Task FailWithRootCauseAsync(this HttpResponseMessage message, string description = "Failed",
    CancellationToken ct = default)
  {
    return message.FailWithRootCauseAsync<object>(description, ct);
  }

  [DoesNotReturn]
  public static async Task<T> FailWithRootCauseAsync<T>(this HttpResponseMessage message,
    string description = "Failed", CancellationToken ct = default)
  {
    var cause = await message.Content.ReadAsStringAsync(ct);
    throw new RaffleFailedException(cause, description);
  }

  public static async Task<string> ReadStringResultOrFailAsync(this HttpResponseMessage message,
    string description = "Failed", CancellationToken ct = default)
  {
    if (message.IsSuccessStatusCode)
    {
      return await message.Content.ReadAsStringAsync(ct);
    }


    return await message.FailWithRootCauseAsync<string>(description, ct);
  }

  public static async Task<string> ReadPossiblyGZippedAsStringAsync(this HttpContent content,
    CancellationToken ct = default)
  {
    if (content.Headers.ContentEncoding.Any(x => x == "gzip"))
    {
      // Decompress manually
      using (var s = await content.ReadAsStreamAsync(ct))
      {
        using (var decompressed = new GZipStream(s, CompressionMode.Decompress))
        {
          using (var rdr = new StreamReader(decompressed))
          {
            return await rdr.ReadToEndAsync();
          }
        }
      }
    }

    // Use standard implementation if not compressed
    return await content.ReadAsStringAsync(ct);
  }
}