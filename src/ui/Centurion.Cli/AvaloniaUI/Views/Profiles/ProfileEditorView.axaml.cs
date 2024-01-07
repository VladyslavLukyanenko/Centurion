using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Profiles;

namespace Centurion.Cli.AvaloniaUI.Views.Profiles;

public class ProfileEditorView : ReactiveUserControl<ProfileEditorViewModel>
{
  public ProfileEditorView()
  {
    InitializeComponent();
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}