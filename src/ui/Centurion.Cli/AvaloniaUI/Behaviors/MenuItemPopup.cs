using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace Centurion.Cli.AvaloniaUI.Behaviors;

public class MenuItemPopup : AvaloniaObject
{
  public static readonly AttachedProperty<Popup?> PopupProperty =
    AvaloniaProperty.RegisterAttached<MenuItemPopup, MenuItem, Popup?>("Popup", default!, false, BindingMode.OneWay);

  public static readonly AttachedProperty<ICommand?> CommandProperty =
    AvaloniaProperty.RegisterAttached<MenuItemPopup, MenuItem, ICommand?>("Command", default!, false,
      BindingMode.OneWay);

  public static readonly AttachedProperty<object?> CommandParameterProperty =
    AvaloniaProperty.RegisterAttached<MenuItemPopup, MenuItem, object?>("CommandParameter", default!, false,
      BindingMode.OneWay);

  static MenuItemPopup()
  {
    PopupProperty.Changed.Subscribe(_ => HandlePopupChanged(_.Sender, _.NewValue.GetValueOrDefault<Popup?>()));
  }

  private static void HandlePopupChanged(IAvaloniaObject sender, Popup? popup)
  {
    if (sender is not MenuItem menuItem)
    {
      return;
    }

    if (popup is null)
    {
      return;
    }

    menuItem.Click -= OnMenuItemOnClick;
    menuItem.Click += OnMenuItemOnClick;

    void OnMenuItemOnClick(object? o, RoutedEventArgs e)
    {
      (menuItem.Parent as IMenuElement)?.Close();
      var cmd = sender.GetValue(CommandProperty);
      var param = sender.GetValue(CommandParameterProperty);
      cmd?.Execute(param);
      popup.Open();
    }
  }

  /// <summary>
  /// Accessor for Attached property <see cref="PopupProperty"/>.
  /// </summary>
  public static void SetPopup(AvaloniaObject element, Popup? value)
  {
    element.SetValue(PopupProperty, value);
  }

  /// <summary>
  /// Accessor for Attached property <see cref="PopupProperty"/>.
  /// </summary>
  public static Popup? GetPopup(AvaloniaObject element)
  {
    return element.GetValue(PopupProperty);
  }

  /// <summary>
  /// Accessor for Attached property <see cref="CommandProperty" />.
  /// </summary>
  public static void SetCommand(AvaloniaObject element, ICommand? value)
  {
    element.SetValue(CommandProperty, value);
  }

  /// <summary>
  /// Accessor for Attached property <see cref="CommandProperty" />.
  /// </summary>
  public static ICommand? GetCommand(AvaloniaObject element)
  {
    return element.GetValue(CommandProperty);
  }

  /// <summary>
  /// Accessor for Attached property <see cref="CommandParameterProperty" />.
  /// </summary>
  public static void SetCommandParameter(AvaloniaObject element, object? value)
  {
    element.SetValue(CommandParameterProperty, value);
  }

  /// <summary>
  /// Accessor for Attached property <see cref="CommandParameterProperty" />.
  /// </summary>
  public static object? GetCommandParameter(AvaloniaObject element)
  {
    return element.GetValue(CommandParameterProperty);
  }
}