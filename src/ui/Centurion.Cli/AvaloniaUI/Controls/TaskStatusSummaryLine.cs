using Centurion.Cli.Core;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class TaskStatusSummaryLine
{
  public TaskStatusSummaryLine(string title, TaskProgress progress, int count)
  {
    Title = title;
    Progress = progress;
    Count = count;
  }

  public string Title { get; private set; }
  public TaskProgress Progress { get; private set; }
  public int Count { get; private set; }
}