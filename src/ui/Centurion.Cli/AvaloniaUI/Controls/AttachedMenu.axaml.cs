using Avalonia;
using Avalonia.Controls;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class AttachedMenu : ContextMenu
{
  /// <summary>
  /// Defines the <see cref="IsShown"/> property.
  /// </summary>
  public static readonly StyledProperty<bool> IsShownProperty =
    AvaloniaProperty.Register<AttachedMenu, bool>(nameof(IsShown));

  static AttachedMenu()
  {
    IsShownProperty.Changed.Subscribe(args =>
    {
      var menu = (AttachedMenu)args.Sender;
      if (menu.IsShown)
      {
        menu.Open();
      }
      else
      {
        menu.Close();
      }
    });

    IsOpenProperty.Changed.Subscribe(args =>
    {
      if (args.Sender is not AttachedMenu menu)
      {
        return;
      }

      menu.IsShown = menu.IsOpen;
    });
  }

  /// <summary>
  /// Gets a value indicating whether the menu is open.
  /// </summary>
  public bool IsShown
  {
    get => GetValue(IsShownProperty);
    set
    {
      if (value == IsShown)
      {
        return;
      }

      SetValue(IsShownProperty, value);
    }
  }
}