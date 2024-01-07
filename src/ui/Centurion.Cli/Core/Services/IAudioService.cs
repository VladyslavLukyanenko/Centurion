namespace Centurion.Cli.Core.Services;

public interface IAudioService
{
  ValueTask Play(string fileName);
}