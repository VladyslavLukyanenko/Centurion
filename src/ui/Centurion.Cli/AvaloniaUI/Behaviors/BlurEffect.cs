using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Centurion.Cli.AvaloniaUI.Controls;

namespace Centurion.Cli.AvaloniaUI.Behaviors;

public class BlurEffect : AvaloniaObject
{
  private static CustomBlurBehind? _blur;

  public static readonly AttachedProperty<Popup?> PopupProperty =
    AvaloniaProperty.RegisterAttached<BlurEffect, IAvaloniaObject, Popup?>("Popup", default!, false, BindingMode.OneWay);

  public static readonly AttachedProperty<CustomBlurBehind?> BlurBehindProperty =
    AvaloniaProperty.RegisterAttached<BlurEffect, IAvaloniaObject, CustomBlurBehind?>("BlurBehind", default!, false, BindingMode.OneWay);

  static BlurEffect()
  {
    PopupProperty.Changed.Subscribe(_ => HandlePopupChanged(_.Sender, _.NewValue.GetValueOrDefault<Popup?>()));
    BlurBehindProperty.Changed.Subscribe(_ =>
      HandleBlurChanged(_.Sender, _.NewValue.GetValueOrDefault<CustomBlurBehind?>()));
  }

  private static void HandlePopupChanged(IAvaloniaObject sender, Popup? popup)
  {
    if (popup is null)
    {
      return;
    }

    popup.Opened += PopupOnOpened;
    popup.Closed += PopupOnClosed;

    void PopupOnOpened(object? _, EventArgs e)
    {
      if (_blur is null)
      {
        return;
      }

      _blur.IsVisible = true;
    }

    void PopupOnClosed(object? _, EventArgs e)
    {
      if (_blur is null)
      {
        return;
      }

      _blur.IsVisible = false;
    }
  }

  private static void HandleBlurChanged(IAvaloniaObject sender, CustomBlurBehind? e)
  {
    _blur = e;
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
  /// Accessor for Attached property <see cref="BlurBehindProperty"/>.
  /// </summary>
  public static void SetBlurBehind(AvaloniaObject element, CustomBlurBehind? value)
  {
    element.SetValue(BlurBehindProperty, value);
  }

  /// <summary>
  /// Accessor for Attached property <see cref="BlurBehindProperty"/>.
  /// </summary>
  public static CustomBlurBehind? GetBlurBehind(AvaloniaObject element)
  {
    return element.GetValue(BlurBehindProperty);
  }
}