using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Centurion.Cli.Core.Services.ToastNotifications;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class NotificationToast : Button
{
  private static readonly string[] SupportedTypes =
    Enum.GetNames(typeof(ToastType)).Select(_ => ":" + _.ToLowerInvariant()).ToArray();

  public static readonly DirectProperty<NotificationToast, ICommand?> CloseCommandProperty =
    AvaloniaProperty.RegisterDirect<NotificationToast, ICommand?>(nameof(CloseCommand),
      button => button.CloseCommand, (button, command) => button.CloseCommand = command, enableDataValidation: true);


  private ICommand? _closeCommand;

  public ICommand? CloseCommand
  {
    get => _closeCommand;
    set => SetAndRaise(CloseCommandProperty, ref _closeCommand, value);
  }


  public static readonly StyledProperty<string> TitleProperty =
    AvaloniaProperty.Register<NotificationToast, string>(nameof(Title));

  public string Title
  {
    get => GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  public static readonly StyledProperty<TimeSpan> LifetimeDurationProperty =
    AvaloniaProperty.Register<NotificationToast, TimeSpan>(nameof(LifetimeDuration));

  public TimeSpan LifetimeDuration
  {
    get => GetValue(LifetimeDurationProperty);
    set => SetValue(LifetimeDurationProperty, value);
  }

  public static readonly StyledProperty<string> MessageContentProperty =
    AvaloniaProperty.Register<NotificationToast, string>(nameof(MessageContent));

  public string MessageContent
  {
    get => GetValue(MessageContentProperty);
    set => SetValue(MessageContentProperty, value);
  }

  public static readonly StyledProperty<string?> CustomIconProperty =
    AvaloniaProperty.Register<NotificationToast, string?>(nameof(CustomIcon));

  public string? CustomIcon
  {
    get => GetValue(CustomIconProperty);
    set => SetValue(CustomIconProperty, value);
  }

  public static readonly StyledProperty<object?> CustomRightContentProperty =
    AvaloniaProperty.Register<NotificationToast, object?>(nameof(CustomRightContent));

  public object? CustomRightContent
  {
    get => GetValue(CustomRightContentProperty);
    set => SetValue(CustomRightContentProperty, value);
  }

  public static readonly StyledProperty<object?> CustomBottomContentProperty =
    AvaloniaProperty.Register<NotificationToast, object?>(nameof(CustomBottomContent));

  public object? CustomBottomContent
  {
    get => GetValue(CustomBottomContentProperty);
    set => SetValue(CustomBottomContentProperty, value);
  }

  public static readonly StyledProperty<ToastType> TypeProperty =
    AvaloniaProperty.Register<NotificationToast, ToastType>(nameof(Type));

  public ToastType Type
  {
    get => GetValue(TypeProperty);
    set
    {
      SetValue(TypeProperty, value);
      foreach (var supportedType in SupportedTypes)
      {
        PseudoClasses.Remove(supportedType);
      }

      PseudoClasses.Add(":" + value.ToString().ToLowerInvariant());
    }
  }
}