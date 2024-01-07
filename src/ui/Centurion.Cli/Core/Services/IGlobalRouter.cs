namespace Centurion.Cli.Core.Services;

public interface IGlobalRouter
{
  ValueTask ShowTransitionView(bool authenticated);
  ValueTask ShowLoginView();
  ValueTask ShowMainView();
}