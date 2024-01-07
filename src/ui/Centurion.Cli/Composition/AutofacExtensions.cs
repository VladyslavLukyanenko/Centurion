using Autofac;
using Autofac.Builder;
using Autofac.Features.Scanning;

namespace Centurion.Cli.Composition;

public static class AutofacExtensions
{
  /// <summary>
  /// Filters the scanned types to include only those in the namespace of the provided type
  /// or one of its sub-namespaces.
  /// </summary>
  /// <param name="registration">Registration to filter types from.</param>
  /// <typeparam name="T">A type in the target namespace.</typeparam>
  /// <returns>Registration builder allowing the registration to be configured.</returns>
  public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
    InNotEmptyNamespaceOf<T>(
      this IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>? registration)
  {
    return registration != null
      ? registration.InNotEmptyNamespace(typeof(T).Namespace)
      : throw new ArgumentNullException(nameof(registration));
  }


  /// <summary>
  /// Filters the scanned types to include only those in the provided namespace
  /// or one of its sub-namespaces.
  /// </summary>
  /// <typeparam name="TLimit">Registration limit type.</typeparam>
  /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
  /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
  /// <param name="registration">Registration to filter types from.</param>
  /// <param name="ns">The namespace from which types will be selected.</param>
  /// <returns>Registration builder allowing the registration to be configured.</returns>
  public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> InNotEmptyNamespace<TLimit,
    TScanningActivatorData, TRegistrationStyle>(
    this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
    string? ns)
    where TScanningActivatorData : ScanningActivatorData
  {
    if (registration == null)
      throw new ArgumentNullException(nameof(registration));

    return registration.Where(t => !string.IsNullOrEmpty(ns) && t.IsInNamespace(ns));
  }
}