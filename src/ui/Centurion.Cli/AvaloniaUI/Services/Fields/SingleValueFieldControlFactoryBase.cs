using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Contracts;

namespace Centurion.Cli.AvaloniaUI.Services.Fields;

public abstract class SingleValueFieldControlFactoryBase<T> : IFieldControlFactory
{
  public virtual bool IsSupported(IConfigFieldAccessor field) => field is IConfigFieldAccessor<T>;

  public virtual Control Create(IConfigFieldAccessor field)
  {
    var panel = new StackPanel();
    var isRequired = (field.Descriptor.Cardinality & (uint)Cardinality.Required) != 0;
    var label = new Label
    {
      Content = field.Descriptor.Name + (isRequired ? " *" : ""),
      Foreground = new SolidColorBrush(Color.Parse("#ff6b78a5")),
      Margin = new Thickness(0, 0, 0, -1),
    };

    panel.Children.Add(label);

    var control = CreateEditorControl((IConfigFieldAccessor<T>)field);
    control.Margin = new Thickness(0, 0, 0, 10);
    if (control is IInputElement e)
    {
      label.Target = e;
    }

    panel.Children.Add(control);

    return panel;
  }

  protected abstract Control CreateEditorControl(IConfigFieldAccessor<T> field);
}