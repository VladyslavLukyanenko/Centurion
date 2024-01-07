using System.Collections;
using System.Reactive.Subjects;
using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Contracts;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Type = Centurion.Contracts.Type;

namespace Centurion.Cli.Core.Services.Modules;

public class FieldAccessorFactory : IFieldAccessorFactory
{
  public IConfigFieldAccessor Create(ConfigFieldDescriptor descriptor, IFieldAccessor fieldAccessor,
    IMessage instance)
  {
    var rawInitialValue = fieldAccessor.GetValue(instance);
    if (descriptor.Type == Type.Bool)
    {
      var initialValue = rawInitialValue is bool b && b;
      var subj = new BehaviorSubject<bool>(initialValue);
      subj.Subscribe(v => fieldAccessor.SetValue(instance, v));
      return new BooleanFieldAccessor(subj, descriptor);
    }

    if (
      descriptor.Type == Type.String
      || descriptor.Type is Type.Float or Type.Double
      || IsUnsignedFixedPointNumber(descriptor)
      || IsSignedFixedPointNumber(descriptor)
      )
    {
      var initialValue = rawInitialValue?.ToString();
      var subj = new BehaviorSubject<string?>(initialValue);
      subj.Subscribe(v => fieldAccessor.SetValue(instance, ConvertStringValueToSourceType(descriptor, v)));
      return new StringFieldAccessor(subj, descriptor);
    }
    if (descriptor.Type == Type.Message && descriptor.TypeName == ".google.protobuf.Duration")
    {
      var initialValue = (Duration?) rawInitialValue ?? Duration.FromTimeSpan(TimeSpan.Zero);
      var subj = new BehaviorSubject<string?>(((long) initialValue.ToTimeSpan().TotalMilliseconds).ToString());
      subj.Subscribe(v =>
      {
        long.TryParse(v, out var ms);
        fieldAccessor.SetValue(instance, Duration.FromTimeSpan(TimeSpan.FromMilliseconds(ms)));
      });
      return new StringFieldAccessor(subj, descriptor);
    }

    var isAllowedValueList = descriptor.Type == Type.Enum && (descriptor.Cardinality & (uint)Cardinality.Repeated) != 0;
    if (isAllowedValueList)
    {
      var initialValue = rawInitialValue;
      var list = (IList)initialValue;
      var enumType = fieldAccessor.Descriptor.EnumType.ClrType;
      var initialValueList = list.Cast<int>()
        .Select(ix => descriptor.AllowedValues.FirstOrDefault(_ => _.Index == ix))
        .ToList();

      var subj = new BehaviorSubject<object?>(initialValueList);
      subj.Subscribe(v =>
      {
        list.Clear();
        foreach (ReflectedAllowedValue item in (IEnumerable)v)
        {
          list.Add(System.Enum.ToObject(enumType, item.Index));
        }
      });

      return new ComplexObjectFieldAccessor(subj, descriptor);
    }

    if (descriptor.Type == Type.Message)
    {
      var initialValue = rawInitialValue;
      var subj = new BehaviorSubject<object?>(initialValue);
      subj.Subscribe(v => fieldAccessor.SetValue(instance, v));
      return new ComplexObjectFieldAccessor(subj, descriptor);
    }

    throw new InvalidOperationException("Not supported field type " + descriptor.Type);
  }

  private object? ConvertStringValueToSourceType(ConfigFieldDescriptor descriptor, string? rawValue)
  {
    return descriptor.Type switch
    {
      Type.String => rawValue,
      Type.Int64 or Type.Sfixed64 => long.TryParse(rawValue ?? "0", out var v) ? v : default,
      Type.Int32 or Type.Sfixed32 => int.TryParse(rawValue ?? "0", out var v) ? v : default,
      Type.Uint64 or Type.Fixed64 => ulong.TryParse(rawValue ?? "0", out var v) ? v : default,
      Type.Uint32 or Type.Fixed32 => uint.TryParse(rawValue ?? "0", out var v) ? v : default,
      Type.Double => double.TryParse(rawValue ?? "0", out var v) ? v : default,
      Type.Float => float.TryParse(rawValue ?? "0", out var v) ? v : default,
      _ => throw new InvalidOperationException("Unsupported type for signed fixed point value " + descriptor.Type)
    };
  }

  private bool IsSignedFixedPointNumber(ConfigFieldDescriptor descriptor) =>
    descriptor.Type is Type.Int64 or Type.Int32 or Type.Sfixed64 or Type.Sfixed32;

  private bool IsUnsignedFixedPointNumber(ConfigFieldDescriptor descriptor) =>
    descriptor.Type is Type.Uint64 or Type.Uint32 or Type.Fixed64 or Type.Fixed32;
}