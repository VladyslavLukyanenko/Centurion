using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using NodaTime;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class NotificationsListBox : ItemsControl
{
  private ICommand _clearCommand;

  /// <summary>
  /// Defines the <see cref="ClearCommand"/> property.
  /// </summary>
  public static readonly DirectProperty<NotificationsListBox, ICommand> ClearCommandProperty =
    AvaloniaProperty.RegisterDirect<NotificationsListBox, ICommand>(nameof(ClearCommand),
      button => button.ClearCommand, (button, command) => button.ClearCommand = command, enableDataValidation: true);

  /// <summary>
  /// Defines the <see cref="CommandParameter"/> property.
  /// </summary>
  public static readonly StyledProperty<object> CommandParameterProperty =
    AvaloniaProperty.Register<Button, object>(nameof(CommandParameter));

  /// <summary>
  /// Gets or sets an <see cref="ICommand"/> to be invoked when the button is clicked.
  /// </summary>
  public ICommand ClearCommand
  {
    get => _clearCommand;
    set => SetAndRaise(ClearCommandProperty, ref _clearCommand, value);
  }

  /// <summary>
  /// Gets or sets a parameter to be passed to the <see cref="ClearCommand"/>.
  /// </summary>
  public object CommandParameter
  {
    get => GetValue(CommandParameterProperty);
    set => SetValue(CommandParameterProperty, value);
  }
}

public class NotificationData : ReactiveObject
{
  [Reactive] public string Title { get; set; }
  [Reactive] public string Description { get; set; }
  [Reactive] public Instant Timestamp { get; set; }
  [Reactive] public NotificationType Type { get; set; }
}

public enum NotificationType
{
  Success,
  Info,
  Warn,
  Error
}