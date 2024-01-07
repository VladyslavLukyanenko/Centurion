namespace Centurion.Monitor.App;

[AttributeUsage(AttributeTargets.Class)]
public class MonitorAttribute : Attribute
{
  public MonitorAttribute(string monitorSlug)
  {
    MonitorSlug = monitorSlug;
  }

  public string MonitorSlug { get; }
}