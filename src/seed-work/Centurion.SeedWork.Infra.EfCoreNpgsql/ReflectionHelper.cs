using System.Collections;
using System.Reflection;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql;
#nullable disable
public static class ReflectionHelper
{
  public static Type GetInternalType(string assemblyName, string classNamespace, string className)
  {
    Assembly assembly = Assembly.Load(assemblyName);
    var type = assembly.GetType($"{classNamespace}.{className}");
    return type;
  }

  public static object GetStaticInternalProperty(string assemblyName, string classNamespace, string className,
    string propertyName)
  {
    return GetInternalType(assemblyName, classNamespace, className)
      .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
      .GetValue(null, null);
  }
  public static object CreateInstanceOfInternalClass(string assemblyName, string classNamespace, string className,
    object[] ctorArgs)
  {
    Assembly assembly = Assembly.Load(assemblyName);
    return assembly.CreateInstance($"{classNamespace}.{className}", false,
      BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance,
      null, ctorArgs, null, null);
  }

  public static bool IsGenericAssignableFrom(Type sourceType, Type baseType)
  {
    return sourceType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseType)
           || sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == baseType;
  }

  public static void CallGenericMethod(string methodName, Type calleeType, Type[] typeParameters,
    object[] arguments)
  {
    var targetMethod = calleeType
      .GetTypeInfo()
      .DeclaredMethods
      .FirstOrDefault(_ => _.Name == methodName);

    if (targetMethod == null)
    {
      throw new InvalidOperationException($"Can't call method {methodName} on {calleeType.Name}");
    }

    targetMethod.MakeGenericMethod(typeParameters)
      .Invoke(calleeType, arguments);
  }

  public static bool IsDictionary(Type t)
  {
    return IsGenericAssignableFrom(t, typeof(IReadOnlyDictionary<,>))
           || IsGenericAssignableFrom(t, typeof(IDictionary<,>))
           || t.IsAssignableFrom(typeof(IDictionary));
  }
}