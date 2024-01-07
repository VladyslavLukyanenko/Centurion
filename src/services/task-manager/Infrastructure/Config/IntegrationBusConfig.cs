namespace Centurion.TaskManager.Infrastructure.Config;

public class IntegrationBusConfig
{
  public string EventsTopic { get; init; } = null!;

  public Uri GetEventsTopicAddress() => GetTopicAddress(EventsTopic);

  private static Uri GetTopicAddress(string name) => new($"exchange:{name}");
}