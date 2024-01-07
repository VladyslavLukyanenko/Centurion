using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Infra.EfMappings;

public static class CommonEntityTypeBuilderExtensions
{
  public static EntityTypeBuilder<T> SetPrivatePropertyFieldAccessMode<T>(this EntityTypeBuilder<T> builder,
    string propName)
    where T : class
  {
    var navigation = builder.Metadata.FindNavigation(propName);
    navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

    return builder;
  }

  public static PropertyBuilder<TEnum> HasEnumerationConversion<TEnum>(this PropertyBuilder<TEnum> self)
    where TEnum : Enumeration
  {
    return self.HasConversion(new ValueConverter<TEnum, int>(_ => _.Id,
      value => value.ToEnumeration<TEnum>()));
  }

  public static PropertyBuilder<TEnum> HasNullableEnumerationConversion<TEnum>(this PropertyBuilder<TEnum> self)
    where TEnum : Enumeration?
  {
    return self.HasConversion(new ValueConverter<TEnum, int?>(_ => _!.Id,
      value => value.HasValue ? value.Value.ToEnumeration<TEnum>() : null!));
  }
}