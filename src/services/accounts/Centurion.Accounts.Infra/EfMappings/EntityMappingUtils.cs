using System.Collections.Concurrent;
using System.Reflection;

namespace Centurion.Accounts.Infra.EfMappings;

internal static class EntityMappingUtils
{
  private static readonly IDictionary<NavRef, string> ResolutionsCache =
    new ConcurrentDictionary<NavRef, string>();

  public static string ResolveNavigationField<TSource, TNav>(string? possibleName = null)
  {
    var sourceType = typeof(TSource);
    var navigationType = typeof(TNav);
    var nav = new NavRef(sourceType, navigationType);
    if (!ResolutionsCache.TryGetValue(nav, out var result))
    {
      result = ResolveNavigationField(possibleName, sourceType, navigationType);
      if (string.IsNullOrEmpty(result))
      {
        throw new InvalidOperationException(
          $"Can't resolve navigation field in {sourceType.Name} of type {navigationType.Name},"
          + $" {nameof(possibleName)}={(string.IsNullOrEmpty(possibleName) ? "Not provided" : possibleName)}");
      }

      ResolutionsCache[nav] = result;
    }

    return result;
  }

  private static string ResolveNavigationField(string? possibleName, Type sourceType, Type navigationType)
  {
    var field = sourceType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
      .Where(_ =>
        !string.IsNullOrEmpty(possibleName)
        && _.Name.Contains(possibleName, StringComparison.InvariantCultureIgnoreCase)
        || _.FieldType.Name.Contains(navigationType.Name, StringComparison.InvariantCultureIgnoreCase))
      .Select(_ => _.Name)
      .FirstOrDefault();
    if (field == null && sourceType.BaseType != null)
    {
      return ResolveNavigationField(possibleName, sourceType.BaseType, navigationType);
    }

    return field ?? throw new InvalidOperationException("Can't resolve navigation field " + possibleName);
  }

  private readonly struct NavRef
    : IEquatable<NavRef>
  {
    public NavRef(Type source, Type nav)
    {
      Source = source;
      Nav = nav;
    }

    private Type Source { get; }
    private Type Nav { get; }

    public bool Equals(NavRef other)
    {
      return Source == other.Source && Nav == other.Nav;
    }

    public override bool Equals(object? obj)
    {
      return obj is NavRef other && Equals(other);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((Source != null ? Source.GetHashCode() : 0) * 397) ^ (Nav != null ? Nav.GetHashCode() : 0);
      }
    }
  }
}