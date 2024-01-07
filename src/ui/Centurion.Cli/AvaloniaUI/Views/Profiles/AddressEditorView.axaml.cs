using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Profiles;

namespace Centurion.Cli.AvaloniaUI.Views.Profiles;

public class AddressView : ReactiveUserControl<AddressEditorViewModel>
{
  public AddressView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}