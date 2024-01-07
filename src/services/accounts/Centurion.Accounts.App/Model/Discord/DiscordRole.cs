using System.Globalization;

namespace Centurion.Accounts.App.Model.Discord;

public class DiscordRole
{
  public ulong Id { get; set; }
  public string Name { get; set; } = null!;
  public int Color { get; set; }

  public string GetHexColor() => "#" + Color.ToString("X6", CultureInfo.InvariantCulture);
}