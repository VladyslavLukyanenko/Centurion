using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.Academy
{
  public class AcademyData
  {
    [JsonPropertyName("online")] public List<AcademyOnlineItemData> Online { get; set; }
  }

  public class AcademyOnlineItemData
  {
    [JsonPropertyName("skuId")] public string SkuId { get; set; }
    [JsonPropertyName("inventoryStatus")] public string InventoryStatus { get; set; }
  }
}