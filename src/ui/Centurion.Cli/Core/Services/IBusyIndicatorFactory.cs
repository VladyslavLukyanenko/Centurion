namespace Centurion.Cli.Core.Services;

public interface IBusyIndicatorFactory
{
  IDisposable SwitchToBusyState();
}