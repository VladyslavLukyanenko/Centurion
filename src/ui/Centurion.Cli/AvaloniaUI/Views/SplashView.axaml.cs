using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace Centurion.Cli.AvaloniaUI.Views;

public class SplashView : Window
{
  private static readonly string[] AuthenticatedMessages =
  {
    "Preparing server, please wait",
    "It shouldn't take much time",
    "Configuring server"
  };

  private static readonly string[] LoadingMessages =
  {
    "Loading. Please wait"
  };

  private readonly Func<Task>? _initializer;
  private readonly string[] _statusMessages;
  private readonly CompositeDisposable _disposable = new();

  public SplashView()
    : this(false)
  {
  }

  public SplashView(bool isAuthenticated)
  {
    InitializeComponent();
    _statusMessages = isAuthenticated ? AuthenticatedMessages : LoadingMessages;

#if DEBUG
    this.AttachDevTools();
#endif
  }

  public SplashView(Func<Task> initializer)
    : this()
  {
    _initializer = initializer;
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }

  protected override void OnClosing(CancelEventArgs e)
  {
    _disposable.Dispose();
    base.OnClosing(e);
  }

  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
    base.OnApplyTemplate(e);

    var ix = 0;
    var tx = this.FindControl<TextBlock>("StatusTextTb");
    tx.Text = _statusMessages[ix++];
    Observable.Interval(TimeSpan.FromSeconds(5))
      .ObserveOn(RxApp.MainThreadScheduler)
      .Subscribe(_ =>
      {
        if (ix >= _statusMessages.Length)
        {
          ix = 0;
        }

        tx.Text = _statusMessages[ix++];
      })
      .DisposeWith(_disposable);

    // var decoration = new Image
    // {
    //   Source = new SvgImage
    //   {
    //     Source = StaticResources.TopWaveBorder
    //   },
    //   VerticalAlignment = VerticalAlignment.Top,
    //   HorizontalAlignment = HorizontalAlignment.Left,
    //   Width = 433,
    //   Height = 511,
    //   Margin = new Thickness(18),
    //   Stretch = Stretch.None
    // };
    //
    // this.FindControl<Grid>("RootContainer")
    //   .Children
    //   .Add(decoration);

    // await Task.Delay(TimeSpan.FromMilliseconds(100));
    // if (_initializer is not null)
    // {
    //   await _initializer();
    // }
    _initializer?.Invoke();
  }
}