using System.Collections;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Contracts;
using DynamicData.Kernel;
using Type = Centurion.Contracts.Type;

namespace Centurion.Cli.AvaloniaUI.Services.Fields;

public class AllowedValuesFieldControlFactory : SingleValueFieldControlFactoryBase<object>
{
  public override bool IsSupported(IConfigFieldAccessor field) =>
    field.Descriptor.Type is Type.Enum && field.Descriptor.AllowedValues.Any();

  protected override Control CreateEditorControl(IConfigFieldAccessor<object> field)
  {
    var initial = (IList)field.GetValue()!;
    var listBox = new ListBox
    {
      Background = new SolidColorBrush(Color.Parse("#ff0c1343")),
      BorderThickness = new Thickness(1),
      BorderBrush = new SolidColorBrush(Color.Parse("#ff111a53")),
      CornerRadius = new CornerRadius(4),
      Padding = new Thickness(1),
      Height = 150,
      SelectionMode = SelectionMode.Toggle | SelectionMode.Multiple,
      Items = field.Descriptor.AllowedValues.AsList().AsReadOnly(),
      SelectedItems = initial,
      Styles =
      {
        new Style(_ => _.OfType(typeof(ListBoxItem)))
        {
          Setters =
          {
            new Setter(TemplatedControl.PaddingProperty, new Thickness(10, 5)),
            new Setter(Layoutable.MarginProperty, new Thickness(0, 1, 0, 0)),
            new Setter(TemplatedControl.CornerRadiusProperty, new CornerRadius(4)),
          }
        }
      },
      ItemTemplate = new FuncDataTemplate<ReflectedAllowedValue>((v, _) => new TextBlock
      {
        FontSize = 10,
        [!TextBlock.TextProperty] = new Binding($"{nameof(v.Value)}.{nameof(v.Value.DisplayName)}")
      })
    };

    // foreach (ReflectedAllowedValue value in initial)
    // {
    //   var index = field.Descriptor.AllowedValues.IndexOf(value);
    //   listBox.Selection.Select(index);
    // }

    listBox.SelectionChanged += ListBoxOnSelectionChanged;

    field.Disposable.Add(Disposable.Create(listBox, lb => { lb.SelectionChanged -= ListBoxOnSelectionChanged; }));

    return listBox;

    void ListBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
      var selectedItems = listBox.SelectedItems.OfType<ReflectedAllowedValue>().ToList();
      // if (listBox.SelectedItems.Count > 1 && )
      // {
      //   selectedItems = selectedItems.Where(_ => !_.Value.DiscardOthers).ToList();
      //   listBox.SelectedItems = selectedItems;
      //   return;
      // }

      if (listBox.SelectedItems.Count > 1)
      {
        var currVal = (IList)field.GetValue()!;
        bool? onlyDiscarded = null;
        if (currVal.OfType<ReflectedAllowedValue>().All(_ => _.Value.DiscardOthers))
        {
          onlyDiscarded = false;
        }
        else if (selectedItems.Any(_ => _.Value.DiscardOthers))
        {
          onlyDiscarded = true;
        }

        if (onlyDiscarded.HasValue)
        {
          selectedItems = selectedItems.Where(_ => _.Value.DiscardOthers == onlyDiscarded).ToList();
          listBox.SelectedItems = selectedItems;
          return;
        }
      }

      field.SetValue(selectedItems);
    }
  }
}