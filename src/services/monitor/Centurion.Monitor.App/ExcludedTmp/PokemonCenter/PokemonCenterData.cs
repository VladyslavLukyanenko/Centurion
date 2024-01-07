using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable
namespace Centurion.Monitor.App.Sites.PokemonCenter
{
  public class PokemonCenterData
  {
    [JsonPropertyName("_availability")] public List<PokemonCenterAvailabilityData> Availability { get; set; }
  }

  public class PokemonCenterAvailabilityData
  {
    [JsonPropertyName("state")] public string State { get; set; }
  }
}