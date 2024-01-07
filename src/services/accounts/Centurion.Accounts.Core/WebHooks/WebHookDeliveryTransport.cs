namespace Centurion.Accounts.Core.WebHooks;

[Flags]
public enum WebHookDeliveryTransport : ushort
{
  RemoteEndpoint = 0b0000_0001,
  DiscordWebHook = 0b0000_0010
}