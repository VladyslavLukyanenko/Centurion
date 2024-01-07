using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Harvesters;

namespace Centurion.Cli.AvaloniaUI.Views;

public partial class HarvesterItemView : ReactiveUserControl<HarvesterItemViewModel>
{
  public HarvesterItemView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}