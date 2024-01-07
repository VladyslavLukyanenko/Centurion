using Avalonia;
using Avalonia.Controls;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class UserAvatarButton : RadioButton
{
  public static readonly StyledProperty<string> PictureProperty =
    AvaloniaProperty.Register<NotificationToast, string>(nameof(Picture));

  public string Picture
  {
    get => GetValue(PictureProperty);
    set => SetValue(PictureProperty, value);
  }
}