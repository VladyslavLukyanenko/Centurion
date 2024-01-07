using Centurion.CloudManager.App.Model;
using Centurion.CloudManager.App.Services;
using MassTransit;

namespace Centurion.CloudManager.Infra.Consumers;

public class ComponentStatsConsumer : IConsumer<ComponentStatsEntry>
{
  private readonly IComponentsStateRegistry _componentsStateRegistry;

  public ComponentStatsConsumer(IComponentsStateRegistry componentsStateRegistry)
  {
    _componentsStateRegistry = componentsStateRegistry;
  }

  public Task Consume(ConsumeContext<ComponentStatsEntry> context)
  {
    _componentsStateRegistry.Update(context.Message);
    return Task.CompletedTask;
  }
}