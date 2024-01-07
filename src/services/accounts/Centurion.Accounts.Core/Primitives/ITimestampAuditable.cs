using NodaTime;

namespace Centurion.Accounts.Core.Primitives;

public interface ITimestampAuditable
{
  Instant CreatedAt { get; }
  Instant UpdatedAt { get; }
}