namespace Centurion.TaskManager.Web;

public static class IdsUtil
{
  private static readonly string EmptyId = Guid.Empty.ToString();

  public static bool IsEmpty(string raw) => string.IsNullOrEmpty(raw) || raw == EmptyId;
}