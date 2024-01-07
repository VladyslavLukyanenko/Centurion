namespace Centurion.TaskManager.Core.Services;

public class UserInfo
{
  public string UserId { get; init; } = null!;
  public string UserName { get; init; } = null!;
  public ulong DiscordId { get; init; }
  public string Avatar { get; init; } = null!;
}