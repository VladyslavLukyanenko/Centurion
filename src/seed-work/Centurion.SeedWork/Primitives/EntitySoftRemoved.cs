using Centurion.SeedWork.Events;
using NodaTime;

namespace Centurion.SeedWork.Primitives;

public class EntitySoftRemoved : DomainEvent
{
  public EntitySoftRemoved(string id, ISoftRemovable source)
  {
    Id = id;
    RemovedAt = source.RemovedAt;
    Type = source.GetType().FullName!;
  }
    
  public string Id { get; }
  public string Type { get; }
  public Instant RemovedAt { get; }
}