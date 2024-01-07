using Avalonia.Controls;
using Centurion.Cli.Core.Services.Modules.Accessors;

namespace Centurion.Cli.AvaloniaUI.Services.Fields;

public interface IFieldControlFactory
{
  bool IsSupported(IConfigFieldAccessor field);
  Control? Create(IConfigFieldAccessor field);
}