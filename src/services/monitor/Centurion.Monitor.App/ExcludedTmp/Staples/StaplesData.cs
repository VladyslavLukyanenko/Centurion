using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.Staples
{
  public class StaplesData
  {
    [JsonPropertyName("skuState")] public StaplesSkuStateData SkuState { get; set; }
  }

  public class StaplesSkuStateData
  {
    [JsonPropertyName("skuData")] public StaplesSkuDataData SkuData { get; set; }
  }

  public class StaplesSkuDataData
  {
    [JsonPropertyName("items")] public List<StaplesSkuItemData> Items { get; set; }
  }

  public class StaplesSkuItemData
  {
    [JsonPropertyName("inventory")] public StaplesInventoryData Inventory { get; set; }
    [JsonPropertyName("product")] public StaplesProductData Product { get; set; }
    [JsonPropertyName("price")] public StaplesPriceData Price { get; set; }
  }

  public class StaplesPriceData
  {
    [JsonPropertyName("item")] public List<StaplesPriceItemData> Item { get; set; }
  }

  public class StaplesPriceItemData
  {
    [JsonPropertyName("finalPrice")] public decimal FinalPrice { get; set; }
  }

  public class StaplesProductData
  {
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("images")] public StaplesImageListData Images { get; set; }
  }

  public class StaplesImageListData
  {
    [JsonPropertyName("standard")] public List<string> Standard { get; set; }
  }

  public class StaplesInventoryData
  {
    [JsonPropertyName("items")] public List<StaplesItemData> Items { get; set; }
  }

  public class StaplesItemData
  {
    [JsonPropertyName("productIsOutOfStock")]
    public bool ProductIsOutOfStock { get; set; }
  }
}