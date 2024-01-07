using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.DicksSportingGoods
{
  public class DicksSportingGoodsData
  {
    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("meta")] public MetaData Meta { get; set; }
    [JsonPropertyName("data")] public PayloadData Data { get; set; }
  }

  public class PayloadData
  {
    [JsonPropertyName("skus")] public List<SkuData> Skus { get; set; }
  }

  public class MetaData
  {
    [JsonPropertyName("key")] public string Key { get; set; }
    [JsonPropertyName("value")] public string Value { get; set; }
  }

  public class SkuData
  {
    [JsonPropertyName("sku")] public string Sku { get; set; }
    [JsonPropertyName("location")] public string Location { get; set; }
    [JsonPropertyName("atsqty")] public string Atsqty { get; set; }
    [JsonPropertyName("isaqty")] public string Isaqty { get; set; }
    [JsonPropertyName("time")] public string Time { get; set; }
  }
}