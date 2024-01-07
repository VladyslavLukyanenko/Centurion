using CSharpFunctionalExtensions;
using NodaTime;

namespace Centurion.SeedWork.Primitives;

public interface ISoftRemovable
{
  Instant RemovedAt { get; }
  bool IsRemoved();
  Result Remove();
  // Result RemoveAt(Instant removedAt);
}