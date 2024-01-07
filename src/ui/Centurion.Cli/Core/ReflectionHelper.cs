using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Centurion.Cli.Core;

public static class ReflectionHelper
{
  private static readonly ConcurrentDictionary<Type, MethodInfo> MethodsCache = new();
  public static MethodInfo GetGenericMethodDef<TSource>(Expression<Action<TSource>> expr)
  {
    var selectorBody = (MethodCallExpression) expr.Body;
    return selectorBody.Method.GetGenericMethodDefinition();
  }
    
    
  public static Type? GetBaseGenericType(this Type curr, Type generic)
  {
    if (curr.IsGenericType && curr.GetGenericTypeDefinition() == generic)
    {
      return curr;
    }

    if (curr.BaseType == null)
    {
      return null;
    }

    return GetBaseGenericType(curr.BaseType, generic);
  }

  public static Type? GetInternalType(string assemblyName, string classNamespace, string className)
  {
    Assembly assembly = Assembly.Load(assemblyName);
    var type = assembly.GetType($"{classNamespace}.{className}");
    return type;
  }

  public static object? ReflectionInvoke(this object self, string methodName, object[]? parameters = null)
  {
    var method = MethodsCache.GetOrAdd(self.GetType(),
      type => type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
              ?? throw new InvalidOperationException($"Can't find method {methodName} on type {type.Name}"));

    return method.Invoke(self, parameters ?? Array.Empty<object>());
  }


  public static object? GetStaticInternalProperty(string assemblyName, string classNamespace, string className,
    string propertyName)
  {
    return GetInternalType(assemblyName, classNamespace, className)?
      .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)?
      .GetValue(null, null);
  }

  public static object? CreateInstanceOfInternalClass(string assemblyName, string classNamespace, string className,
    object[] ctorArgs)
  {
    Assembly assembly = Assembly.Load(assemblyName);
    return assembly.CreateInstance($"{classNamespace}.{className}", false,
      BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance,
      null, ctorArgs, null, null);
  }
}