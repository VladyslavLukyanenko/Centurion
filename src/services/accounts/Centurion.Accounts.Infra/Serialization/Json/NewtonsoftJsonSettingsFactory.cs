using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Centurion.Accounts.Infra.Serialization.Json;

public static class NewtonsoftJsonSettingsFactory
{
  public static Func<JsonSerializerSettings> CreateSettingsProvider(IContractResolver contractResolver,
    JsonSerializerSettings? settings = null)
  {
    settings ??= JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();
    ConfigureSettingsWithDefaults(settings, contractResolver);
    return SettingsProvider;

    JsonSerializerSettings SettingsProvider()
    {
      return settings;
    }
  }

  public static void ConfigureSettingsWithDefaults(JsonSerializerSettings settings,
    IContractResolver? contractResolver = null)
  {
    settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
    settings.NullValueHandling = NullValueHandling.Ignore;
    settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
    settings.DateParseHandling = DateParseHandling.DateTimeOffset;
    settings.ContractResolver = contractResolver ?? new CamelCasePropertyNamesContractResolver();
    settings.Converters.Add(new StringEnumConverter());
    settings.Converters.Add(new IBinaryDataJsonConverter());
    settings.Converters.Add(new WebHookPayloadSerializer());

    settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
  }
}