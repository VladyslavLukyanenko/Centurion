using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Centurion.Cli.Core.ViewModels;

namespace Centurion.Cli.AvaloniaUI;

public class ViewLocator : IDataTemplate
{
  public IControl Build(object data)
  {
    if (data is ViewModelBase vm)
    {
      return (IControl) ReactiveUI.ViewLocator.Current.ResolveView(vm)!;
    }

    var name = data.GetType().FullName!.Replace("ViewModel", "View");
    var type = Type.GetType(name);

    if (type != null)
    {
      return (Control)Activator.CreateInstance(type)!;
    }
    else
    {
      return new TextBlock { Text = "Not Found: " + name };
    }
  }

  public bool Match(object data)
  {
    return data is ViewModelBase;
  }
}