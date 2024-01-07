using Centurion.CloudManager.App.Model;

namespace Centurion.CloudManager.App.Services;

public interface IComponentsStateRegistry
{
  ILookup<string, ComponentStatsEntry> GetGroupedEntries(TimeSpan maxRefreshDelay);
  void Update(ComponentStatsEntry entry);
}