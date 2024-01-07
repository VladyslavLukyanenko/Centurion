using NodaTime;

namespace Centurion.Accounts.Core.Events;

public interface IIntegrationEvent
{
  Instant Timestamp { get; }
}