using Avalonia.Controls;
using Centurion.Cli.AvaloniaUI.Services.Fields;
using Centurion.Cli.Core.Domain.Fields;
using Centurion.Cli.Core.Services.Modules.Accessors;

namespace Centurion.Cli.AvaloniaUI.Services;

public class FieldPresentationManager : IFieldPresentationManager
{
  private readonly IEnumerable<IFieldControlFactory> _factories;

  public FieldPresentationManager(IEnumerable<IFieldControlFactory> factories)
  {
    _factories = factories;
  }

  public void DisplayFields(StackPanel surface, IEnumerable<IConfigFieldAccessor> fields, int maxItemsPerRow = 1)
  {
    surface.Children.Clear();
    var controls = ConstructControls(fields);
    PlaceControls(surface, controls.regular, controls.fullRowControls, maxItemsPerRow);
  }

  private void PlaceControls(StackPanel surface, IList<Control> regularControls,
    IEnumerable<Control> fullRowControls, int maxItemsPerRow)
  {
    var rowsCount = (int)Math.Ceiling(regularControls.Count / (double)maxItemsPerRow);
    for (int rowIdx = 0; rowIdx < rowsCount; rowIdx++)
    {
      var colsCount = Math.Min(regularControls.Count - rowIdx * maxItemsPerRow, maxItemsPerRow);
      var grid = new Grid();
      for (int cIdx = 0; cIdx < colsCount * 2 - 1; cIdx++)
      {
        grid.ColumnDefinitions.Add(new ColumnDefinition
        {
          Width = cIdx % 2 == 0
            ? new GridLength(1, GridUnitType.Star)
            : new GridLength(20)
        });
      }

      for (int colIdx = 0; colIdx < colsCount; colIdx++)
      {
        var fieldIdx = rowIdx * maxItemsPerRow + colIdx;
        var field = regularControls[fieldIdx];
        grid.Children.Add(field);
        Grid.SetColumn(field, colIdx * 2);
      }

      surface.Children.Add(grid);
    }

    foreach (var control in fullRowControls)
    {
      surface.Children.Add(control);
    }
  }

  private (List<Control> regular, List<Control> fullRowControls) ConstructControls(
    IEnumerable<IConfigFieldAccessor> fields)
  {
    var controls = new List<Control>();
    var fullRowControls = new List<Control>();
    foreach (IConfigFieldAccessor field in fields)
    {
      var controlsFactory = _factories.FirstOrDefault(_ => _.IsSupported(field));
      if (controlsFactory == null)
      {
        throw new InvalidOperationException($"Can't find factory for field type '{field.GetType().Name}'");
      }

      var item = controlsFactory.Create(field);
      // NOTICE: we can skip some fields (hidden for example)
      if (item is not null)
      {
        if (field is TemplatedMultilineField)
        {
          fullRowControls.Add(item);
        }
        else
        {
          controls.Add(item);
        }
      }
    }

    return (controls, fullRowControls);
  }
}