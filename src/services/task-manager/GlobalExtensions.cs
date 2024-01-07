namespace Centurion.TaskManager;

public static class GlobalExtensions
{
  public static Guid ToGuidOrEmpty(this string? rawGuid)
  {
    if (string.IsNullOrEmpty(rawGuid))
    {
      return default;
    }

    Guid.TryParse(rawGuid, out var g);
    return g;
  }
}