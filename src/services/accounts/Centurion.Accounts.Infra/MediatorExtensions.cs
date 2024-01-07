using Microsoft.EntityFrameworkCore;
using Centurion.Accounts.Core.Events;
using Centurion.Accounts.Core.Primitives;

namespace Centurion.Accounts.Infra;

public static class MediatorExtensions
{
  public static async Task DispatchDomainEvents(this IMessageDispatcher messageDispatcher, DbContext context)
  {
    var domainEntries = context.ChangeTracker
      .Entries<IEventSource>()
      .Where(_ => _.Entity.DomainEvents.Any())
      .ToList();

    var domainEvents = domainEntries.SelectMany(_ => _.Entity.DomainEvents)
      .ToList();

    domainEntries.ForEach(entry => entry.Entity.ClearDomainEvents());

    foreach (IDomainEvent domainEvent in domainEvents)
    {
      await messageDispatcher.PublishEventAsync(domainEvent);
    }
  }
}