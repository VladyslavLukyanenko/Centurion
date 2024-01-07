namespace Centurion.Accounts.Core.Events;

public interface IMessageDispatcher
{
  ValueTask PublishEventAsync<T>(T @event, CancellationToken ct = default);
  ValueTask SendCommandAsync<T>(T @event, CancellationToken ct = default);
}