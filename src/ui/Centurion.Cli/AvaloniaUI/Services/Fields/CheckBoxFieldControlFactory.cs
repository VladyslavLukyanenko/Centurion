using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Services.Modules.Accessors;
using DynamicData.Binding;

namespace Centurion.Cli.AvaloniaUI.Services.Fields;

public class CheckBoxFieldControlFactory : SingleValueFieldControlFactoryBase<bool>
{
  protected override Control CreateEditorControl(IConfigFieldAccessor<bool> field)
  {
    var checkBox = new CheckBox
    {
      Content = field.Descriptor.Name,
      IsChecked = (bool) field.GetValue()!
    };

    checkBox.WhenValueChanged(_ => _.IsChecked)
      .Select(isChecked => isChecked == true)
      .DistinctUntilChanged()
      .Subscribe(field.Source.OnNext)
      .DisposeWith(field.Disposable);
    
    return checkBox;
  }
}