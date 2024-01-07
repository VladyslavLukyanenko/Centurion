using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

#nullable enable

namespace Centurion.UI.Themes.Nobleman;

public enum NoblemanThemeMode
{
  Light,
  Dark,
}

/// <summary>
/// Includes the fluent theme in an application.
/// </summary>
public class NoblemanTheme : IStyle, IResourceProvider
{
  private readonly Uri _baseUri;
  private IStyle[]? _loaded;
  private bool _isLoading;

  /// <summary>
  /// Initializes a new instance of the <see cref="NoblemanTheme"/> class.
  /// </summary>
  /// <param name="baseUri">The base URL for the XAML context.</param>
  public NoblemanTheme(Uri baseUri)
  {
    _baseUri = baseUri;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="NoblemanTheme"/> class.
  /// </summary>
  /// <param name="serviceProvider">The XAML service provider.</param>
  public NoblemanTheme(IServiceProvider serviceProvider)
  {
    _baseUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext))!).BaseUri;
  }

  /// <summary>
  /// Gets or sets the mode of the fluent theme (light, dark).
  /// </summary>
  public NoblemanThemeMode Mode { get; set; }

  public IResourceHost? Owner => (Loaded as IResourceProvider)?.Owner;

  /// <summary>
  /// Gets the loaded style.
  /// </summary>
  public IStyle Loaded
  {
    get
    {
      if (_loaded == null)
      {
        _isLoading = true;
        var loaded = (IStyle)AvaloniaXamlLoader.Load(GetUri(), _baseUri);
        _loaded = new[] { loaded };
        _isLoading = false;
      }

      return _loaded?[0]!;
    }
  }

  bool IResourceNode.HasResources => (Loaded as IResourceProvider)?.HasResources ?? false;

  IReadOnlyList<IStyle> IStyle.Children => _loaded ?? Array.Empty<IStyle>();

  public event EventHandler OwnerChanged
  {
    add
    {
      if (Loaded is IResourceProvider rp)
      {
        rp.OwnerChanged += value;
      }
    }
    remove
    {
      if (Loaded is IResourceProvider rp)
      {
        rp.OwnerChanged -= value;
      }
    }
  }

  public SelectorMatchResult TryAttach(IStyleable target, IStyleHost? host) => Loaded.TryAttach(target, host);

  public bool TryGetResource(object key, out object? value)
  {
    if (!_isLoading && Loaded is IResourceProvider p)
    {
      return p.TryGetResource(key, out value);
    }

    value = null;
    return false;
  }

  void IResourceProvider.AddOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.AddOwner(owner);
  void IResourceProvider.RemoveOwner(IResourceHost owner) => (Loaded as IResourceProvider)?.RemoveOwner(owner);

  private Uri GetUri() => Mode switch
  {
    NoblemanThemeMode.Dark => new Uri("avares://Centurion.UI.Themes.Nobleman/NoblemanDark.xaml", UriKind.Absolute),
    _ => new Uri("avares://Centurion.UI.Themes.Nobleman/NoblemanLight.xaml", UriKind.Absolute),
  };
}