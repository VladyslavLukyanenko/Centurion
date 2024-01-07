using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.HomeDepot
{
  public class HomeDepotData
  {
    [JsonPropertyName("data")] public HomeDepotPayloadData Data { get; set; }
  }

  public class HomeDepotPayloadData
  {
    [JsonPropertyName("mediaPriceInventory")]
    public HomeDepotMediaPriceInventoryData MediaPriceInventory { get; set; }
  }

  public class HomeDepotMediaPriceInventoryData
  {
    [JsonPropertyName("productDetailsList")]
    public List<HomeDepotProductData> ProductDetailsList { get; set; }
  }

  public class HomeDepotProductData
  {
    [JsonPropertyName("onlineInventory")] public HomeDepotOnlineInventoryData OnlineInventory { get; set; }
    [JsonPropertyName("imageLocation")] public string ImageLocation { get; set; }
  }

  public class HomeDepotOnlineInventoryData
  {
    [JsonPropertyName("enableItem")] public bool EnableItem { get; set; }
  }
}