using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Centurion.Accounts.Core.WebHooks;

namespace Centurion.Accounts.Infra.Serialization.Json;

public class WebHookPayloadSerializer : JsonConverter<WebHookPayload>
{
  private static readonly IList<PropertyInfo> Props =
    typeof(WebHookPayload).GetTypeInfo().DeclaredProperties.ToArray();

  private static readonly PropertyInfo DataProp = Props.First(_ => _.Name == nameof(WebHookPayload.Data));

  public override void WriteJson(JsonWriter writer, WebHookPayload? value, JsonSerializer serializer)
  {
    if (value == null)
    {
      writer.WriteNull();
      return;
    }

    writer.WriteStartObject();

    writer.WritePropertyName("type");
    writer.WriteValue(value.EventType);

    writer.WritePropertyName("signature");
    writer.WriteValue(value.Signature);

    writer.WritePropertyName("data");
    writer.WriteRawValue(value.Data);

    writer.WriteEndObject();
  }

  public override WebHookPayload ReadJson(JsonReader reader, Type objectType, WebHookPayload? existingValue,
    bool hasExistingValue, JsonSerializer serializer)
  {
    if (!hasExistingValue)
    {
      existingValue = WebHookPayload.CreateEmpty();
    }

    var jobj = JObject.Load(reader);
    foreach (var property in jobj.Properties())
    {
      if (property.Name.Equals(nameof(WebHookPayload.Data), StringComparison.OrdinalIgnoreCase))
      {
        DataProp.SetValue(existingValue, property.ToString(Formatting.None));
      }
      else
      {
        var propInfo = Props.First(_ => _.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
          
        var valueReader = property.Value.CreateReader();
        valueReader.Read();
        propInfo.SetValue(existingValue, valueReader.Value);
      }
    }

    return existingValue!;
  }
}