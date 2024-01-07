using Avalonia.Controls;
using ReactiveUI;

namespace Centurion.Cli.AvaloniaUI;

public static class ViewModelBaseExtensions
{

  public static Control LocateGuiView(this ReactiveObject viewModel)
  {
    var view = ReactiveUI.ViewLocator.Current.ResolveView(viewModel)!;
    view.ViewModel = viewModel;
    return (Control)view;
  }
}