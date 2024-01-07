using Avalonia.Controls;
using Centurion.Cli.Core.Services.Modules.Accessors;
using DynamicData.Binding;

namespace Centurion.Cli.AvaloniaUI.Services.Fields;

public class TextFieldControlFactory : SingleValueFieldControlFactoryBase<string>
{
  protected override Control CreateEditorControl(IConfigFieldAccessor<string> field)
  {
    var textBox = new TextBox
    {
      Text = field.GetValue()?.ToString(),
      Watermark = field.Descriptor.Name
    };

    textBox.WhenValueChanged(_ => _.Text)
      .Subscribe(field.SetValue);

    return textBox;
  }
}