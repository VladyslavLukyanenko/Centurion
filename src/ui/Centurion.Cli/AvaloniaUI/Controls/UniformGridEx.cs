using Avalonia;
using Avalonia.Controls;

namespace Centurion.Cli.AvaloniaUI.Controls;

/// <summary>
/// A <see cref="Panel"/> with uniform column and row sizes.
/// </summary>
public class UniformGridEx : Panel
{
  /// <summary>
  /// Defines the <see cref="Rows"/> property.
  /// </summary>
  public static readonly StyledProperty<int> RowsProperty =
    AvaloniaProperty.Register<UniformGridEx, int>(nameof(Rows));

  /// <summary>
  /// Defines the <see cref="Columns"/> property.
  /// </summary>
  public static readonly StyledProperty<int> ColumnsProperty =
    AvaloniaProperty.Register<UniformGridEx, int>(nameof(Columns));

  /// <summary>
  /// Defines the <see cref="Spacing"/> property.
  /// </summary>
  public static readonly StyledProperty<int> SpacingProperty =
    AvaloniaProperty.Register<UniformGridEx, int>(nameof(Spacing));

  /// <summary>
  /// Defines the <see cref="FirstColumn"/> property.
  /// </summary>
  public static readonly StyledProperty<int> FirstColumnProperty =
    AvaloniaProperty.Register<UniformGridEx, int>(nameof(FirstColumn));

  private int _rows;
  private int _columns;

  static UniformGridEx()
  {
    AffectsMeasure<UniformGridEx>(RowsProperty, ColumnsProperty, FirstColumnProperty, SpacingProperty);
  }

  /// <summary>
  /// Specifies the row count. If set to 0, row count will be calculated automatically.
  /// </summary>
  public int Rows
  {
    get => GetValue(RowsProperty);
    set => SetValue(RowsProperty, value);
  }

  /// <summary>
  /// Specifies the column count. If set to 0, column count will be calculated automatically.
  /// </summary>
  public int Columns
  {
    get => GetValue(ColumnsProperty);
    set => SetValue(ColumnsProperty, value);
  }

  public int Spacing
  {
    get => GetValue(SpacingProperty);
    set => SetValue(SpacingProperty, value);
  }

  /// <summary>
  /// Specifies, for the first row, the column where the items should start.
  /// </summary>
  public int FirstColumn
  {
    get => GetValue(FirstColumnProperty);
    set => SetValue(FirstColumnProperty, value);
  }

  protected override Size MeasureOverride(Size availableSize)
  {
    UpdateRowsAndColumns();

    var maxWidth = 0d;
    var maxHeight = 0d;

    var availableWidth = availableSize.Width - ColSpacingTotal;
    var availableHeight = availableSize.Height - RowSpacingTotal;

    var childAvailableSize = new Size(availableWidth / _columns, availableHeight / _rows);
    foreach (var child in Children)
    {
      child.Measure(childAvailableSize);
      if (child.DesiredSize.Width > maxWidth)
      {
        maxWidth = child.DesiredSize.Width;
      }

      if (child.DesiredSize.Height > maxHeight)
      {
        maxHeight = child.DesiredSize.Height;
      }
    }

    return new Size(maxWidth * _columns, maxHeight * _rows);
  }

  protected override Size ArrangeOverride(Size finalSize)
  {
    var x = FirstColumn;
    var y = 0;

    var availableWidth = finalSize.Width - ColSpacingTotal;
    var availableHeight = finalSize.Height - RowSpacingTotal;
    var width = availableWidth / _columns;
    var height = availableHeight / _rows;

    var ix = 0;
    foreach (var child in Children)
    {
      if (!child.IsVisible)
      {
        continue;
      }

      child.Arrange(new Rect(x * width + ix * Spacing, y * height + y * Spacing, width, height));

      x++;
      ix++;

      if (x >= _columns)
      {
        ix = 0;
        x = 0;
        y++;
      }
    }

    return finalSize;
  }

  private double ColSpacingTotal => (_columns - 1) * Spacing;
  private double RowSpacingTotal => (_rows - 1) * Spacing;

  private void UpdateRowsAndColumns()
  {
    _rows = Rows;
    _columns = Columns;

    if (FirstColumn >= Columns)
    {
      FirstColumn = 0;
    }

    var itemCount = FirstColumn;

    foreach (var child in Children)
    {
      if (child.IsVisible)
      {
        itemCount++;
      }
    }

    if (_rows == 0)
    {
      if (_columns == 0)
      {
        _rows = _columns = (int)Math.Ceiling(Math.Sqrt(itemCount));
      }
      else
      {
        _rows = Math.DivRem(itemCount, _columns, out int rem);

        if (rem != 0)
        {
          _rows++;
        }
      }
    }
    else if (_columns == 0)
    {
      _columns = Math.DivRem(itemCount, _rows, out int rem);

      if (rem != 0)
      {
        _columns++;
      }
    }
  }
}
