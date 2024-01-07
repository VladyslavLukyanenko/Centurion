namespace Centurion.SeedWork.Events;

public interface IMessageDispatcher
{
  ValueTask PublishEventAsync<T>(T @event, CancellationToken ct = default);
  ValueTask SendCommandAsync<T>(T @event, CancellationToken ct = default);
}