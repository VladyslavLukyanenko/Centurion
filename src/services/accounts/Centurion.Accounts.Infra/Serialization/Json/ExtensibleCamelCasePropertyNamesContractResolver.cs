using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Centurion.Accounts.App.Serialization.NewtonsoftJson.Converters;
using Centurion.Accounts.App.Services;

namespace Centurion.Accounts.Infra.Serialization.Json;

public class ExtensibleCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
{
  private readonly IDictionary<Type, Func<DataConverterAttributeBase, JsonConverter>> _converters;

  public ExtensibleCamelCasePropertyNamesContractResolver(IPathsService pathsService, IConfiguration cfg)
  {
    _converters = new Dictionary<Type, Func<DataConverterAttributeBase, JsonConverter>>
    {
      {
        typeof(UploadedFilePathAttribute), CreateForUploadedFilePath
      }
    };

    JsonConverter CreateForUploadedFilePath(DataConverterAttributeBase attr)
    {
      var key = ((UploadedFilePathAttribute) attr).FallbackPictureKey;
      return new UploadedFilePathToAbsoluteUrlJsonConverter(CreateFallbackPicFactory(key, cfg), pathsService);
    }

    static Func<string?> CreateFallbackPicFactory(string? cfgKey, IConfiguration cfg)
    {
      if (string.IsNullOrEmpty(cfgKey))
      {
        return () => null;
      }

      return () => cfg[cfgKey];
    }
  }

  protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
  {
    var prop = base.CreateProperty(member, memberSerialization);
    var dataConvertAttr = member.GetCustomAttributes<DataConverterAttributeBase>().SingleOrDefault();
    if (dataConvertAttr == null)
    {
      return prop;
    }

    if (_converters.TryGetValue(dataConvertAttr.GetType(), out var converter)
        && !(member.DeclaringType == typeof(VersionWrapper) || member.DeclaringType == typeof(ReadOnlyVersionWrapper))
       )
    {
      IValueProvider wrappingValueProvider;
      if (prop.Writable)
      {
        wrappingValueProvider = new VersionWrapperProvider(prop.ValueProvider!);
      }
      else
      {
        wrappingValueProvider = new ReadOnlyVersionWrapperProvider(prop.ValueProvider!);
      }

      prop.ValueProvider = wrappingValueProvider;
      prop.ObjectCreationHandling = ObjectCreationHandling.Reuse;
      prop.Converter = new VersionWrapperJsonConverter(converter(dataConvertAttr), prop.PropertyType!);
      var wrapperType = prop.Writable ? typeof(VersionWrapper) : typeof(ReadOnlyVersionWrapper);
      prop.PropertyType = wrapperType;
    }

    return prop;
  }

  class VersionWrapperJsonConverter : JsonConverter
  {
    private readonly JsonConverter _impl;
    private readonly Type _valueType;

    public VersionWrapperJsonConverter(JsonConverter impl, Type valueType)
    {
      _impl = impl;
      _valueType = valueType;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
      object? sourceValue = value is VersionWrapper wv
        ? wv.Object
        : ((ReadOnlyVersionWrapper) value!).Object;

      _impl.WriteJson(writer, sourceValue, serializer);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
      JsonSerializer serializer)
    {
      object? sourceValue = existingValue is VersionWrapper wv
        ? wv.Object
        : ((ReadOnlyVersionWrapper) existingValue!).Object;

      return _impl.ReadJson(reader, _valueType, sourceValue, serializer);
    }

    public override bool CanConvert(Type objectType) => _impl.CanConvert(objectType);
  }

  class VersionWrapperProvider : IValueProvider
  {
    readonly IValueProvider _baseProvider;

    public VersionWrapperProvider(IValueProvider baseProvider)
    {
      if (baseProvider is null)
      {
        throw new ArgumentNullException();
      }

      _baseProvider = baseProvider;
    }

    public object GetValue(object target)
    {
      return new VersionWrapper(target, _baseProvider);
    }

    public void SetValue(object target, object? value)
    {
      if (value is VersionWrapper w)
      {
        value = w.Object;
      }

      _baseProvider.SetValue(target, value);
    }
  }

  class ReadOnlyVersionWrapperProvider : IValueProvider
  {
    readonly IValueProvider _baseProvider;

    public ReadOnlyVersionWrapperProvider(IValueProvider baseProvider)
    {
      if (baseProvider is null)
      {
        throw new ArgumentNullException();
      }

      _baseProvider = baseProvider;
    }

    public object GetValue(object target)
    {
      return new ReadOnlyVersionWrapper(target, _baseProvider);
    }

    public void SetValue(object target, object? value)
    {
    }
  }
}


internal class VersionWrapper
{
  readonly object _target;
  readonly IValueProvider _baseProvider;

  public VersionWrapper(object target, IValueProvider baseProvider)
  {
    _target = target;
    _baseProvider = baseProvider;
  }

  public int Version => 1;

  [JsonProperty(NullValueHandling = NullValueHandling.Include)]
  public object? Object
  {
    get => _baseProvider.GetValue(_target);
    set => _baseProvider.SetValue(_target, value);
  }
}

internal class ReadOnlyVersionWrapper
{
  readonly object _target;
  readonly IValueProvider _baseProvider;

  public ReadOnlyVersionWrapper(object target, IValueProvider baseProvider)
  {
    _target = target;
    _baseProvider = baseProvider;
  }

  public int Version => 1;

  [JsonProperty(NullValueHandling = NullValueHandling.Include)]
  public object? Object => _baseProvider.GetValue(_target);
}