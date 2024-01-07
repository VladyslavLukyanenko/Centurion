using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Core;

public static class CoreTypesExtensions
{
  private static readonly Type EntityTypeDef = typeof(IEntity<>);
    
  public static bool IsEntity(this Type type)
  {
    return type.GetInterfaces()
      .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == EntityTypeDef);
  }
}