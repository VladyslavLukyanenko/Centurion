using NodaTime;

namespace Centurion.SeedWork.Primitives;

public interface ITimestampAuditable
{
  Instant CreatedAt { get; }
  Instant UpdatedAt { get; }
}