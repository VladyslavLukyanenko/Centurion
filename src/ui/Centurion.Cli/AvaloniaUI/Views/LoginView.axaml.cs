using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Svg.Skia;
using Centurion.Cli.Core.ViewModels;
using ReactiveUI;

namespace Centurion.Cli.AvaloniaUI.Views;

public class LoginView : ReactiveWindow<LoginViewModel>
{
  public LoginView()
  {
    InitializeComponent();
#if DEBUG
    this.AttachDevTools();
#endif

    this.WhenActivated(d => { ViewModel!.LoadStoredKeyCommand.Execute().Subscribe().DisposeWith(d); });
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }

  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
    base.OnApplyTemplate(e);

    var dashboardIcon = this.FindControl<Image>("DashboardIcon");
    dashboardIcon.Source = new SvgImage
    {
      Source = StaticResources.ArrowTopRight
    };
  }
}