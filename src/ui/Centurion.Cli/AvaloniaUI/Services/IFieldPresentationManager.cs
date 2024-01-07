using Avalonia.Controls;
using Centurion.Cli.Core.Services.Modules.Accessors;

namespace Centurion.Cli.AvaloniaUI.Services;

public interface IFieldPresentationManager
{
  void DisplayFields(StackPanel surface, IEnumerable<IConfigFieldAccessor> fields, int maxItemsPerRow = 1);
}