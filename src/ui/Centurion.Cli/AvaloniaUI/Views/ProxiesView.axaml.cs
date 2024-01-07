using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Proxies;

namespace Centurion.Cli.AvaloniaUI.Views;

public class ProxiesView : ReactiveUserControl<ProxiesViewModel>
{
  public ProxiesView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}