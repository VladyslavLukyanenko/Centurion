using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels;

namespace Centurion.Cli.AvaloniaUI.Views;

public class MainView : ReactiveWindow<MainViewModel>
{
  public MainView()
  {
    InitializeComponent();
#if DEBUG
    this.AttachDevTools();
#endif
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }

  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
    base.OnApplyTemplate(e);
    var mainNav = this.FindControl<StackPanel>("MainNavigation");
    var navigation = ViewModel!.MainNavigation;
    var radioButtons = navigation.Select(item => new RadioButton
      {
        Content = item.Title,
        Command = ViewModel.NavigateToPageCommand,
        CommandParameter = item.ViewModel,
        GroupName = "MainNavigation",
        Classes = Classes.Parse("MainNavigationItem")
      })
      .ToArray();
    mainNav.Children.InsertRange(0, radioButtons);

    var fBtn = radioButtons[0];
    fBtn.IsChecked = true;
    if (fBtn.CommandParameter is not null)
    {
      fBtn.Command.Execute(fBtn.CommandParameter);
    }
  }
}