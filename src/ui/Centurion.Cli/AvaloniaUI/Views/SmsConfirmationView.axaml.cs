using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels;
using ReactiveUI;

namespace Centurion.Cli.AvaloniaUI.Views;

public class SmsConfirmationView : ReactiveWindow<SmsConfirmationViewModel>
{
  public SmsConfirmationView()
  {
    InitializeComponent();
#if DEBUG
    this.AttachDevTools();
#endif

    this.WhenActivated(d =>
    {
      this.WhenAnyValue(_ => _.ViewModel)
        .Where(vm => vm is not null)
        .Select(vm => vm!.CancelCommand.Merge(vm.ConfirmCommand))
        .Switch()
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(_ => Close())
        .DisposeWith(d);
    });
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}