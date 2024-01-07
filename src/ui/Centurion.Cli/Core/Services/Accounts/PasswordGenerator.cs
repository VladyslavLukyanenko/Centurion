using System.Text;

namespace Centurion.Cli.Core.Services.Accounts;

public class PasswordGenerator : IPasswordGenerator
{
  private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  private static readonly Random Rnd = new Random((int) DateTime.Now.Ticks);

  public string Generate(int len)
  {
    var builder = new StringBuilder();
      
    for (var i = 0; i < len; i++)
    {
      builder.Append(Characters[Rnd.Next(0, Characters.Length)]);
    }

    return builder.ToString();
  }
}