namespace Centurion.Cli.Core.Services.ToastNotifications;

public class ToastContent
{
  public ToastType Type { get; }
  public string Content { get; }
  public string Title { get; }
  private Action? _clickHandler;
  public TimeSpan AutoCloseTimeout { get; }
  public ToastPriority Priority { get; }

  public string? CustomIcon { get; }
  public object? RightContent { get; }
  public object? BottomContent { get; }

  public ToastContent(string content, string title, ToastType type, Action? clickHandler, TimeSpan autoCloseTimeout,
    ToastPriority priority, string? customIcon, object? rightContent, object? bottomContent)
  {
    Content = content;
    Title = title;
    Type = type;
    _clickHandler = clickHandler;
    AutoCloseTimeout = autoCloseTimeout;
    Priority = priority;
    CustomIcon = customIcon;
    RightContent = rightContent;
    BottomContent = bottomContent;
  }

  public static ToastContent Information(string content, string title = "Attention",
    ToastPriority priority = ToastPriority.Normal, string? customIcon = null, object? rightContent = null,
    object? bottomContent = null) =>
    new(content, title, ToastType.Information, null, TimeSpan.FromSeconds(1.5), priority, null, null, null);

  public static ToastContent Success(string content, string title = "Success",
    ToastPriority priority = ToastPriority.Normal, string? customIcon = null, object? rightContent = null,
    object? bottomContent = null) =>
    new(content, title, ToastType.Success, null, TimeSpan.FromSeconds(3), priority, null, null, null);

  public static ToastContent Warning(string content, string title = "Caution",
    ToastPriority priority = ToastPriority.Normal, string? customIcon = null, object? rightContent = null,
    object? bottomContent = null) =>
    new(content, title, ToastType.Warning, null, TimeSpan.FromSeconds(5), priority, null, null, null);

  public static ToastContent Error(string content, string title = "Error",
    ToastPriority priority = ToastPriority.Normal, string? customIcon = null, object? rightContent = null,
    object? bottomContent = null) =>
    new(content, title, ToastType.Error, null, TimeSpan.FromSeconds(10), priority, null, null, null);
}