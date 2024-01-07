using System.Collections;
using System.Reflection;
using System.Runtime.Loader;

namespace Centurion.Accounts.Infra;

public static class ReflectionHelper
{
  private static IList<Assembly> LoadAppAssemblies(this Assembly assembly)
  {
    return assembly.GetReferencedAssemblies()
      .Where(a => a.FullName.ToLowerInvariant().Contains("centurion"))
      // .Where(a => AssemblyLoadContext.Default.Assemblies.All(la => la.GetName().Name != a.Name))
      .Select(AssemblyLoadContext.Default.LoadFromAssemblyName)
      .Union(new[] {assembly})
      .ToArray();
  }

  private static readonly Lazy<Assembly[]> _assembliesProvider = new Lazy<Assembly[]>(() =>
  {
    var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();


    // todo: make this shit OK
    var assemblies = entryAssembly
      .LoadAppAssemblies()
      .SelectMany(_ => _.LoadAppAssemblies()
        .SelectMany(i => i.LoadAppAssemblies()))
      .GroupBy(_ => _.GetName().FullName)
      .Select(_ => _.First())
      .ToList();

    if (!assemblies.Contains(entryAssembly))
    {
      assemblies.Add(entryAssembly);
    }

    return assemblies.ToArray();
  });

  public static Assembly[] ApplicationAssemblies => _assembliesProvider.Value;

  public static IList<Type> GetTypes(string startNamespace, Type baseType)
  {
    return Assembly.GetAssembly(baseType)
      ?.GetTypes()
//            return AppDomain.CurrentDomain.GetAssemblies()
//                .SelectMany(_ => _.GetTypes())
      .Where(t => t.IsClass && !t.IsAbstract)
      .Where(t => !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith(startNamespace))
      .Where(baseType.IsAssignableFrom)
      .ToArray() ?? Array.Empty<Type>();
  }

  public static IList<Type> GetTypesOfGeneric(Type baseType, string? startNamespace = null)
  {
    return ApplicationAssemblies
      .SelectMany(_ => _.ExportedTypes)
//            return AppDomain.CurrentDomain.GetAssemblies()
//                .SelectMany(_ => _.GetTypes())
      .Where(t => t.IsClass && !t.IsAbstract)
      .Where(t => string.IsNullOrEmpty(startNamespace)
                  || !string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith(startNamespace))
      .Where(t => IsGenericAssignableFrom(t, baseType))
      .ToList();
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