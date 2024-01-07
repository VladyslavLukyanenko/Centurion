using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Tasks;

namespace Centurion.Cli.AvaloniaUI.Views.Tasks;

public class TasksView : ReactiveUserControl<TasksViewModel>
{
  public TasksView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}