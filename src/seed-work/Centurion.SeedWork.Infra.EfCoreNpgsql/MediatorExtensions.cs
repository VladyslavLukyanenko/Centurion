using Centurion.SeedWork.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql;

public static class MediatorExtensions
{
  public static async Task DispatchDomainEvents(this IMediator mediator, DbContext context)
  {
    if (!context.ChangeTracker.HasChanges())
    {
      return;
    }

    var domainEntries = context.ChangeTracker
      .Entries<IEventSource>()
      .Where(_ => _.Entity.DomainEvents.Any())
      .ToList();

    var domainEvents = domainEntries.SelectMany(_ => _.Entity.DomainEvents)
      .ToList();

    domainEntries.ForEach(entry => entry.Entity.ClearDomainEvents());

    foreach (INotification domainEvent in domainEvents)
    {
      await mediator.Publish(domainEvent);
    }

    //            
//            var tasks = domainEvents
//                .Select(async domainEvent => await mediator.Publish(domainEvent))
//                .ToList();
//
//            await Task.WhenAll(tasks);
  }
}
