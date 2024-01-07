using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Profiles;

namespace Centurion.Cli.AvaloniaUI.Views;

public class ProfilesView : ReactiveUserControl<ProfilesViewModel>
{
  public ProfilesView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}