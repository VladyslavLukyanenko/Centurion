using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.NewEgg
{
  public class NewEggData
  {
    [JsonPropertyName("MainItem")] public NewEggMainItemData MainItem { get; set; }
  }

  public class NewEggMainItemData
  {
    [JsonPropertyName("Instock")] public bool Instock { get; set; }
    [JsonPropertyName("Description")] public NewEggDescriptionData Description { get; set; }
    [JsonPropertyName("FinalPrice")] public decimal FinalPrice { get; set; }
    [JsonPropertyName("Image")] public NewEggImageData Image { get; set; }
  }

  public class NewEggDescriptionData
  {
    [JsonPropertyName("Title")] public string Title { get; set; }
  }

  public class NewEggImageData
  {
    [JsonPropertyName("Normal")] public NewEggImageTypeData Normal { get; set; }
  }

  public class NewEggImageTypeData
  {
    [JsonPropertyName("ImageName")] public string ImageName { get; set; }
  }
}