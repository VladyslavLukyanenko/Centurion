using System.Collections.Concurrent;
using System.Reflection;

namespace Centurion.Accounts.App.WebHooks;

public static class WebHookDataInspector
{
  private static readonly ConcurrentDictionary<Type, string> TypesCache = new();

  public static string GetType(WebHookDataBase data)
  {
    var dataType = data.GetType();
    if (!TypesCache.TryGetValue(dataType, out var type))
    {
      var typeAttr = dataType.GetCustomAttribute<WebHookTypeAttribute>();
      if (typeAttr == null)
      {
        throw new InvalidOperationException("Can't get data type. Missing WebHookType attribute");
      }

      type = typeAttr.Name;
      TypesCache[dataType] = type;
    }

    return type;
  }
}