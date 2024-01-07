using System;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.Velveeta
{
  public partial class VelveetaData
  {
    [JsonPropertyName("result")] public VelveetaResultData Result { get; set; }
  }

  public partial class VelveetaResultData
  {
    [JsonPropertyName("expires_seconds")] public long ExpiresSeconds { get; set; }

    [JsonPropertyName("value")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Value { get; set; }

    [JsonPropertyName("minute_dt")] public string MinuteDt { get; set; }

    [JsonPropertyName("expires")] public long Expires { get; set; }

    [JsonPropertyName("minute")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Minute { get; set; }
  }
}