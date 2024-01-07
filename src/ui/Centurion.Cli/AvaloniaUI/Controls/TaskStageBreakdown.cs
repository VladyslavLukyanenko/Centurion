using Centurion.Cli.Core;
using DynamicData.Binding;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class TaskStageBreakdown
{
  public TaskStageBreakdown(string title)
  {
    Title = title;
  }

  public string Title { get; private set; }
  public ObservableCollectionExtended<TaskStatusSummaryLine> Lines { get; } = new();

  public void UpdateLines(IEnumerable<StatusAwareTask> tasks)
  {
    using var __ = Lines.SuspendNotifications();
    Lines.Clear();

    var lines = tasks.GroupBy(_ => _.Status)
      .Select(g => new TaskStatusSummaryLine(g.Key.Title, g.Key.ToProgress(), g.Count()))
      .OrderBy(_ => _.Title);

    Lines.AddRange(lines);
  }
}