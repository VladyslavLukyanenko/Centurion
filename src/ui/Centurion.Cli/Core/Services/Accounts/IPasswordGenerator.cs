namespace Centurion.Cli.Core.Services.Accounts;

public interface IPasswordGenerator
{
  string Generate(int len);
}