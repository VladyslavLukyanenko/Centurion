using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Harvesters;

namespace Centurion.Cli.AvaloniaUI.Views;

public class HarvestersView : ReactiveUserControl<HarvestersViewModel>
{
  public HarvestersView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}