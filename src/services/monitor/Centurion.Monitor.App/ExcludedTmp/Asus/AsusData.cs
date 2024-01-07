using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.Asus
{
  public record AsusData
  {
    [JsonPropertyName("status")] public bool Status { get; set; }
    [JsonPropertyName("data")] public Dictionary<string, AsusDataItemData> Data { get; set; }
  }

  public class AsusDataItemData
  {
    [JsonPropertyName("market_info")] public MarketInfoData MarketInfo { get; set; }
  }

  public class MarketInfoData
  {
    [JsonPropertyName("buy")] public bool Buy { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("seo_photo")] public string Photo { get; set; }
    [JsonPropertyName("price")] public PriceData Price { get; set; }
  }

  public class PriceData
  {
    [JsonPropertyName("final_price")] public FinalPriceData FinalPrice { get; set; }
  }

  public class FinalPriceData
  {
    [JsonPropertyName("price")] public decimal Price { get; set; }
  }
}