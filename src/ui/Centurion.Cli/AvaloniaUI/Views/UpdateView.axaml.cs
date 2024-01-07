using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels;
using ReactiveUI;

namespace Centurion.Cli.AvaloniaUI.Views;

public class UpdateView : ReactiveUserControl<UpdateViewModel>
{
  public UpdateView()
  {
    InitializeComponent();
    this.WhenActivated(d =>
    {
      ViewModel?.LaunchUpdaterCommand?.Subscribe(_ =>
        {
          Application.Current!.Shutdown();
        })
        .DisposeWith(d);
    });
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}