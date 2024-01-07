using DiscordRPC;

namespace Centurion.Cli.Core.Services;

public class DiscordRpcManager : IDiscordRPCManager
{
  private DiscordRpcClient? _rpcClient;

  public void UpdateState(string state)
  {
      
    if (_rpcClient?.IsInitialized ?? false)
    {
      return;
    }
      
    _rpcClient = new DiscordRpcClient("877504787405496320");
    _rpcClient.Initialize();
    _rpcClient.SetPresence(new RichPresence
    {
      Details = $"Centurion v{AppInfo.CurrentAppVersion}",
      State = state,
      Timestamps = Timestamps.Now,
      Assets = new Assets
      {
        LargeImageKey = "centurion_logo",
        LargeImageText = "Centurion",
      },
      Buttons = new Button[]
      {
        new()
        {
          Label = "Twitter",
          Url = "https://twitter.com/CenturionBots"
        },
        new()
        {
          Label = "Website",
          Url = "https://centurion.gg"
        }
      }
    });
  }
}