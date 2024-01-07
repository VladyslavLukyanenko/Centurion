using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Tasks;

namespace Centurion.Cli.AvaloniaUI.Views.Tasks;

public class TaskGroupView : ReactiveUserControl<TaskGroupViewModel>
{
  public TaskGroupView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}