using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class GridLikeListBox : ListBox
{
  protected override IItemContainerGenerator CreateItemContainerGenerator()
  {
    return new GridLikeListBoxItemContainerGenerator(this, ContentControl.ContentProperty,
      ContentControl.ContentTemplateProperty);
  }

  private class GridLikeListBoxItemContainerGenerator : ItemContainerGenerator<GridLikeListBoxItem>
  {
    public GridLikeListBoxItemContainerGenerator(IControl owner, AvaloniaProperty contentProperty,
      AvaloniaProperty contentTemplateProperty)
      : base(owner, contentProperty, contentTemplateProperty)
    {
      Materialized += OnMaterialized;
    }

    private void OnMaterialized(object? sender, ItemContainerEventArgs e)
    {
      foreach (var info in e.Containers)
      {
        info.ContainerControl.SetValue(GridLikeListBoxItem.IndexProperty, info.Index);
      }
    }

    /// <inheritdoc/>
    public override bool TryRecycle(int oldIndex, int newIndex, object item)
    {
      var container = ContainerFromIndex(oldIndex);

      if (container == null)
      {
        throw new IndexOutOfRangeException("Could not recycle container: not materialized.");
      }

      container.SetValue(GridLikeListBoxItem.IndexProperty, newIndex);
      container.SetValue(ContentProperty, item);

      if (item is not IControl)
      {
        container.DataContext = item;
      }

      var info = MoveContainer(oldIndex, newIndex, item);
      RaiseRecycled(new ItemContainerEventArgs(info));

      return true;
    }
  }
}

[PseudoClasses(OddPseudoclass)]
public class GridLikeListBoxItem : ListBoxItem
{
  public const string OddPseudoclass = ":odd";

  public static readonly StyledProperty<int> IndexProperty =
    AvaloniaProperty.Register<GridLikeListBoxItem, int>(nameof(Index), -1);

  static GridLikeListBoxItem()
  {
    IndexProperty.Changed.Subscribe(e =>
    {
      var item = (GridLikeListBoxItem)e.Sender;
      item.PseudoClasses.Remove(OddPseudoclass);
      if ((item.Index + 1) % 2 != 0)
      {
        item.PseudoClasses.Add(OddPseudoclass);
      }
    });
  }

  public int Index
  {
    get => GetValue(IndexProperty);
    set => SetValue(IndexProperty, value);
  }
}

public class ScrollPropagatingStackPanel : Grid, ILogicalScrollable, IScrollable
{
  // private IScrollable _parent = null!;
  private ILogicalScrollable _childPresenter = null!;

  protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
  {
    // if (Parent is not IScrollable s)
    // {
    //   throw new InvalidOperationException("Parent must be scrollable");
    // }
    //
    // _parent = s;

    _childPresenter = Children.OfType<ItemsPresenter>().FirstOrDefault()
                      ?? throw new InvalidOperationException("Can't find ItemsPresenter in direct children");
  }

  public Size Extent => _childPresenter.Extent.WithHeight(_childPresenter.Extent.Height + 2);

  public Vector Offset
  {
    get => _childPresenter.Offset;
    set => _childPresenter.Offset = value;
  }

  public Size Viewport => _childPresenter.Viewport;

  bool ILogicalScrollable.BringIntoView(IControl target, Rect targetRect) =>
    _childPresenter.BringIntoView(target, targetRect);

  IControl ILogicalScrollable.GetControlInDirection(NavigationDirection direction, IControl @from) =>
    _childPresenter.GetControlInDirection(direction, from);

  void ILogicalScrollable.RaiseScrollInvalidated(EventArgs e)
  {
    _childPresenter.RaiseScrollInvalidated(e);
  }

  bool ILogicalScrollable.CanHorizontallyScroll
  {
    get => _childPresenter.CanHorizontallyScroll;
    set => _childPresenter.CanHorizontallyScroll = value;
  }

  bool ILogicalScrollable.CanVerticallyScroll
  {
    get => _childPresenter.CanVerticallyScroll;
    set => _childPresenter.CanVerticallyScroll = value;
  }

  bool ILogicalScrollable.IsLogicalScrollEnabled => _childPresenter.IsLogicalScrollEnabled;

  Size ILogicalScrollable.ScrollSize => _childPresenter.ScrollSize;

  Size ILogicalScrollable.PageScrollSize => _childPresenter.PageScrollSize;

  event EventHandler? ILogicalScrollable.ScrollInvalidated
  {
    add => _childPresenter.ScrollInvalidated += value;
    remove => _childPresenter.ScrollInvalidated -= value;
  }
}