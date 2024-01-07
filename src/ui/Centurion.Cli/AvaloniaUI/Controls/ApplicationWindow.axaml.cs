using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Svg.Skia;
using Centurion.Cli.AvaloniaUI.Views;

namespace Centurion.Cli.AvaloniaUI.Controls;

internal class ApplicationWindowBorderColorToOpacityValConverter : IValueConverter
{
  public static readonly IValueConverter Instance = new ApplicationWindowBorderColorToOpacityValConverter();

  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value is true ? 1 : 0;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    return value is 1;
  }
}

public class ApplicationWindow : ContentControl
{
  private Grid? _decorsContainer;

  private readonly Image _waveBranch = new()
  {
    Source = new SvgImage
    {
      Source = StaticResources.TopWaveBorderBranch
    },
    VerticalAlignment = VerticalAlignment.Top,
    HorizontalAlignment = HorizontalAlignment.Left,
    Width = 240,
    Height = 41,
    Margin = new Thickness(151, 69 /* minus container's margins*/, 0, 0),
    Stretch = Stretch.Uniform
  };

  private readonly Image _wave = new()
  {
    Source = new SvgImage
    {
      Source = StaticResources.TopWaveBorder
    },
    VerticalAlignment = VerticalAlignment.Top,
    HorizontalAlignment = HorizontalAlignment.Left,
    Width = 364,
    Height = 429,
    Margin = new Thickness(0),
    Stretch = Stretch.Uniform
  };


  public static readonly StyledProperty<int> TitleBarHeightProperty =
    AvaloniaProperty.Register<ApplicationWindow, int>(nameof(TitleBarHeight), 38);

  public static readonly StyledProperty<Control> TitleBarContentProperty =
    AvaloniaProperty.Register<ApplicationWindow, Control>(nameof(TitleBarContent));

  public static readonly StyledProperty<Control> TitleBarAdditionalControlsProperty =
    AvaloniaProperty.Register<ApplicationWindow, Control>(nameof(TitleBarAdditionalControls));

  public static readonly StyledProperty<Control> TitleBarAppIconProperty =
    AvaloniaProperty.Register<ApplicationWindow, Control>(nameof(TitleBarAppIcon));

  public static readonly StyledProperty<bool> ShowTitleBarDecorationsProperty =
    AvaloniaProperty.Register<ApplicationWindow, bool>(nameof(ShowTitleBarDecorations),
      notifying: OnShowTitleBarDecorationsChanged);

  public static readonly StyledProperty<bool> ShowTitleBarDelimProperty =
    AvaloniaProperty.Register<ApplicationWindow, bool>(nameof(ShowTitleBarDelim));

  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
    base.OnApplyTemplate(e);

    var titlebar = e.NameScope.Get<Grid>("PART_TitleBar");
    _minimizeButton = e.NameScope.Get<Button>("PART_MinimizeButton");
    // maximizeButton = this.FindControl<Button>("MaximizeButton");
    // maximizeIcon = this.FindControl<Path>("MaximizeIcon");
    // maximizeToolTip = this.FindControl<ToolTip>("MaximizeToolTip");
    _closeButton = e.NameScope.Get<Button>("PART_CloseButton");
    // windowIcon = this.FindControl<Image>("WindowIcon");

    _minimizeButton.Click += MinimizeWindow;
    // maximizeButton.Click += MaximizeWindow;
    _closeButton.Click += CloseWindow;
    // windowIcon.DoubleTapped += CloseWindow;

    titlebar.PointerPressed += BeginListenForDrag;
    titlebar.PointerMoved += HandlePotentialDrag;
    titlebar.PointerReleased += HandlePotentialDrop;
    Background = Brushes.Transparent;

    // SubscribeToWindowState();


    _decorsContainer = e.NameScope.Get<Grid>("PART_DecorationsContainer");
    ToggleTitleBarDecors(ShowTitleBarDecorations);
  }

  // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
  private Button? _minimizeButton;

  // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
  private Button? _closeButton;

  // private Button maximizeButton;
  // private Path maximizeIcon;
  // private ToolTip maximizeToolTip;
  // private Image windowIcon;

  private bool _isPointerPressed;
  private PixelPoint _startPosition = new(0, 0);
  private Point _mouseOffsetToOrigin = new(0, 0);
  private Window ParentWindow => (Window)VisualRoot!;

  private void HandlePotentialDrop(object? sender, PointerReleasedEventArgs e)
  {
    var pos = e.GetPosition(this);
    _startPosition = new PixelPoint((int)(_startPosition.X + pos.X - _mouseOffsetToOrigin.X),
      (int)(_startPosition.Y + pos.Y - _mouseOffsetToOrigin.Y));
    (ParentWindow).Position = _startPosition;
    _isPointerPressed = false;
  }

  private void HandlePotentialDrag(object? sender, PointerEventArgs e)
  {
    if (_isPointerPressed)
    {
      var pos = e.GetPosition(this);
      _startPosition = new PixelPoint((int)(_startPosition.X + pos.X - _mouseOffsetToOrigin.X),
        (int)(_startPosition.Y + pos.Y - _mouseOffsetToOrigin.Y));
      (ParentWindow).Position = _startPosition;
    }
  }

  private void BeginListenForDrag(object? sender, PointerPressedEventArgs e)
  {
    _startPosition = (ParentWindow).Position;
    _mouseOffsetToOrigin = e.GetPosition(this);
    _isPointerPressed = true;
  }

  private void CloseWindow(object? sender, RoutedEventArgs e)
  {
    Window hostWindow = ParentWindow;
    hostWindow.Close();
  }

  /*
  private void MaximizeWindow(object? sender, RoutedEventArgs e)
  {
    Window hostWindow = ParentWindow;

    if (hostWindow.WindowState == WindowState.Normal)
    {
      hostWindow.WindowState = WindowState.Maximized;
    }
    else
    {
      hostWindow.WindowState = WindowState.Normal;
    }
  }*/

  private void MinimizeWindow(object? sender, RoutedEventArgs e)
  {
    Window hostWindow = ParentWindow;
    hostWindow.WindowState = WindowState.Minimized;
  }

  private static void OnShowTitleBarDecorationsChanged(IAvaloniaObject obj, bool show)
  {
    var self = (ApplicationWindow)obj;
    self.ToggleTitleBarDecors(show);
  }

  private void ToggleTitleBarDecors(bool show)
  {
    if (_decorsContainer is null)
    {
      return;
    }

    var children = _decorsContainer.Children;
    if (show)
    {
      Grid.SetRowSpan(_wave, 2);
      Grid.SetRowSpan(_waveBranch, 2);
      Grid.SetColumnSpan(_wave, 3);
      Grid.SetColumnSpan(_waveBranch, 3);

      children.Add(_wave);
      children.Add(_waveBranch);
    }
    else
    {
      children.Remove(_wave);
      children.Remove(_waveBranch);
    }
  }

  // private async void SubscribeToWindowState()
  // {
  //   Window hostWindow = ParentWindow;
  //
  //   while (hostWindow is null)
  //   {
  //     hostWindow = ParentWindow;
  //     await Task.Delay(50);
  //   }
  //
  //   hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s =>
  //   {
  //     if (s != WindowState.Maximized)
  //     {
  //       maximizeIcon.Data =
  //         Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
  //       hostWindow.Padding = new Thickness(0, 0, 0, 0);
  //       maximizeToolTip.Content = "Maximize";
  //     }
  //
  //     if (s == WindowState.Maximized)
  //     {
  //       maximizeIcon.Data = Geometry.Parse(
  //         "M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
  //       hostWindow.Padding = new Thickness(7, 7, 7, 7);
  //       maximizeToolTip.Content = "Restore Down";
  //
  //       // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
  //       /*hostWindow.Padding = new Thickness(
  //               hostWindow.OffScreenMargin.Left,
  //               hostWindow.OffScreenMargin.Top,
  //               hostWindow.OffScreenMargin.Right,
  //               hostWindow.OffScreenMargin.Bottom);*/
  //     }
  //   });
  // }


  public int TitleBarHeight
  {
    get => GetValue(TitleBarHeightProperty);
    set => SetValue(TitleBarHeightProperty, value);
  }

  public bool ShowTitleBarDecorations
  {
    get => GetValue(ShowTitleBarDecorationsProperty);
    set => SetValue(ShowTitleBarDecorationsProperty, value);
  }

  public bool ShowTitleBarDelim
  {
    get => GetValue(ShowTitleBarDelimProperty);
    set => SetValue(ShowTitleBarDelimProperty, value);
  }

  public Control TitleBarContent
  {
    get => GetValue(TitleBarContentProperty);
    set => SetValue(TitleBarContentProperty, value);
  }

  public Control TitleBarAdditionalControls
  {
    get => GetValue(TitleBarAdditionalControlsProperty);
    set => SetValue(TitleBarAdditionalControlsProperty, value);
  }

  public Control TitleBarAppIcon
  {
    get => GetValue(TitleBarAppIconProperty);
    set => SetValue(TitleBarAppIconProperty, value);
  }
}