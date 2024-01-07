namespace Centurion.Cli.Core.Services;

public interface ISmsConfirmationManager
{
  ValueTask<string?> Prompt(string taskDisplayId, string phoneNumber);
}