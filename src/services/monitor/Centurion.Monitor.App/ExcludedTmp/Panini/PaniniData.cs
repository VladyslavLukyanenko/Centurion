using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.Panini
{
  public class PaniniData
  {
    [JsonPropertyName("data")] public PaniniPayloadData Data { get; set; }
  }

  public class PaniniPayloadData
  {
    [JsonPropertyName("item")] public PaniniItemData Item { get; set; }
  }

  public class PaniniItemData
  {
    [JsonPropertyName("is_in_stock")] public bool IsInStock { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
  }
}