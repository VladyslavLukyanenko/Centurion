using System.Reflection;
using Autofac;
using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Contracts;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using ModuleExtensions = Centurion.Contracts.ModuleExtensions;

namespace Centurion.Cli.Core.Services.Modules;

public class ProtoReflectionBasedModuleReflector : IModuleReflector
{
  private static readonly Lazy<IDictionary<string, ModuleInfo>> DescriptorsLazy;
  internal static IDictionary<string, ModuleInfo> DescriptorsDict => DescriptorsLazy.Value;

  static ProtoReflectionBasedModuleReflector()
  {
    DescriptorsLazy = new Lazy<IDictionary<string, ModuleInfo>>(static () =>
    {
      var messages = Assembly.GetExecutingAssembly()
        .DefinedTypes
        .Where(t => t.IsAssignableTo<IMessage>());
      var result = new Dictionary<string, ModuleInfo>();
      foreach (var typeInfo in messages)
      {
        var descriptor = (MessageDescriptor)typeInfo
          .GetProperty(nameof(IMessage.Descriptor), BindingFlags.Public | BindingFlags.Static)
          !.GetValue(null)!;
        var parser = (MessageParser)typeInfo
          .GetProperty(nameof(ModuleMetadata.Parser), BindingFlags.Public | BindingFlags.Static)
          !.GetValue(null)!;

        var protoTypeName = ToFullProtoTypeName(descriptor.FullName);
        result[protoTypeName] = new ModuleInfo
        {
          Descriptor = descriptor,
          Parser = parser
        };
      }

      return result;
    });
  }

  private readonly IFieldAccessorFactory _fieldAccessorFactory;

  public ProtoReflectionBasedModuleReflector(IFieldAccessorFactory fieldAccessorFactory)
  {
    _fieldAccessorFactory = fieldAccessorFactory;
  }

  public IMessage CreateEmpty(ConfigDescriptor descriptor)
  {
    var info = GetModuleInfo(descriptor.MessageType);
    return (IMessage)Activator.CreateInstance(info.Descriptor.ClrType)!;
  }

  public CheckoutModeMetadata? GetSelectedModeOrDefault(IMessage moduleConfig, ModuleMetadata meta)
  {
    var modesOneof = GetModesOneofDescriptor(meta.Config.MessageType);

    var modeName = modesOneof.Fields
      .FirstOrDefault(_ => _.Accessor.HasValue(moduleConfig))
      ?.MessageType
      .FullName;

    if (!string.IsNullOrEmpty(modeName))
    {
      modeName = ToFullProtoTypeName(modeName);
    }

    return meta.Modes.FirstOrDefault(_ => _.Config.MessageType == modeName);
  }

  private static OneofDescriptor GetModesOneofDescriptor(string messageType)
  {
    var info = GetModuleInfo(messageType);

    var modesOneof = info.Descriptor.Oneofs
                       .FirstOrDefault(_ => _.GetOptions().GetExtension(ModuleExtensions.CenturionModuleModes))
                     ?? throw new InvalidOperationException("Can't find modes of module " + messageType);
    return modesOneof;
  }

  public IReadOnlyList<IConfigFieldAccessor> CreateFieldAccessors(ConfigDescriptor descriptor, IMessage instance)
  {
    var info = GetModuleInfo(descriptor.MessageType);
    var accessors = new List<IConfigFieldAccessor>(descriptor.Fields.Count);
    foreach (var field in info.Descriptor.Fields.InDeclarationOrder())
    {
      var describedField = descriptor.Fields.FirstOrDefault(_ => _.Name == field.Name);
      if (describedField is null)
      {
        continue;
      }

      var accessor = _fieldAccessorFactory.Create(describedField, field.Accessor, instance);
      accessors.Add(accessor);
    }

    return accessors;
  }

  public IMessage Create(ConfigDescriptor config, byte[] rawConfig)
  {
    var info = GetModuleInfo(config.MessageType);

    return info.Parser.ParseFrom(rawConfig);
  }

  public IMessage GetOrSet(IMessage moduleConfig, ConfigDescriptor descriptor)
  {
    var modesOneof = GetModesOneofDescriptor(ToFullProtoTypeName(moduleConfig.Descriptor.FullName));
    var settedField = modesOneof.Accessor.GetCaseFieldDescriptor(moduleConfig);
    if (settedField is not null && ToFullProtoTypeName(settedField.MessageType.FullName) == descriptor.MessageType)
    {
      return (IMessage) settedField.Accessor.GetValue(moduleConfig);
    }

    var targetField =
      modesOneof.Fields.First(_ => ToFullProtoTypeName(_.MessageType.FullName) == descriptor.MessageType);
    var mode = CreateEmpty(descriptor);
    targetField.Accessor.SetValue(moduleConfig, mode);

    return mode;
  }

  private static ModuleInfo GetModuleInfo(string messageType)
  {
    if (!DescriptorsDict.TryGetValue(messageType, out var info))
    {
      throw new InvalidOperationException("Can't find config descriptor for " + messageType);
    }

    return info;
  }

  private static string ToFullProtoTypeName(string name) => "." + name;
}