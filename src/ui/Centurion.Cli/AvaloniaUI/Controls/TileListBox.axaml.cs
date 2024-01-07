using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Metadata;

namespace Centurion.Cli.AvaloniaUI.Controls;

public class TileListBox : ListBox
{
  protected override IItemContainerGenerator CreateItemContainerGenerator()
  {
    return new TileListBoxItemContainerGenerator(this, ContentControl.ContentProperty,
      ContentControl.ContentTemplateProperty);
  }
  private class TileListBoxItemContainerGenerator : ItemContainerGenerator<TileListBoxItem>
  {
    public TileListBoxItemContainerGenerator(IControl owner, AvaloniaProperty contentProperty,
      AvaloniaProperty contentTemplateProperty)
      : base(owner, contentProperty, contentTemplateProperty)
    {
      Materialized += OnMaterialized;
    }

    private void OnMaterialized(object? sender, ItemContainerEventArgs e)
    {
      foreach (var info in e.Containers)
      {
        info.ContainerControl.SetValue(TileListBoxItem.IndexProperty, info.Index);
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

      container.SetValue(TileListBoxItem.IndexProperty, newIndex);
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
public class TileListBoxItem : ListBoxItem
{
  public const string OddPseudoclass = ":odd";

  public static readonly StyledProperty<int> IndexProperty =
    AvaloniaProperty.Register<TileListBoxItem, int>(nameof(Index), -1);

  static TileListBoxItem()
  {
    IndexProperty.Changed.Subscribe(e =>
    {
      var item = (TileListBoxItem)e.Sender;
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