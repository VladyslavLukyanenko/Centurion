using Avalonia;
using Avalonia.Controls.Primitives;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class EditorPopup : Popup
{
  /// <summary>
  /// Defines the <see cref="Title"/> property.
  /// </summary>
  public static readonly StyledProperty<string> TitleProperty =
    AvaloniaProperty.Register<AttachedMenu, string>(nameof(Title));

  public string Title
  {
    get => GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }

  /// <summary>
  /// Defines the <see cref="Title"/> property.
  /// </summary>
  public static readonly StyledProperty<object> MainContentProperty =
    AvaloniaProperty.Register<AttachedMenu, object>(nameof(MainContent));

  public object MainContent
  {
    get => GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }
}