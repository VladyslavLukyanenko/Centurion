using Centurion.Accounts.Core.Events;

namespace Centurion.Accounts.Core.Primitives;

public class EntitySoftRemoved : DomainEvent
{
  public EntitySoftRemoved(ISoftRemovable source)
  {
    Source = source;
  }
    
  public ISoftRemovable Source { get; }
}