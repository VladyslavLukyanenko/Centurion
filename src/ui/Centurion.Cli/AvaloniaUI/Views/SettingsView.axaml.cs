using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels;

namespace Centurion.Cli.AvaloniaUI.Views;

public class SettingsView : ReactiveUserControl<SettingsViewModel>
{
  public SettingsView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}