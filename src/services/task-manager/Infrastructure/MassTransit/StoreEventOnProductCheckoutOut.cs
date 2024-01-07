using AutoMapper;
using Centurion.Contracts.Checkout.Integration;
using Centurion.SeedWork.Primitives;
using Centurion.TaskManager.Core.Events;
using Centurion.TaskManager.Web.Hubs;
using Elastic.Apm.Api;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Centurion.TaskManager.Infrastructure.MassTransit;

public class StoreEventOnProductCheckoutOut : IConsumer<ProductCheckedOut>
{
  private readonly ITracer _tracer;
  private readonly IEventRepository _eventRepository;
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;

  // todo: arch decomposition violation. refactor it
  private readonly IHubContext<TaskHub, ITaskHubClient> _taskHubCtx;

  public StoreEventOnProductCheckoutOut(ITracer tracer, IEventRepository eventRepository, IMapper mapper,
    IUnitOfWork unitOfWork, IHubContext<TaskHub, ITaskHubClient> taskHubCtx)
  {
    _tracer = tracer;
    _eventRepository = eventRepository;
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _taskHubCtx = taskHubCtx;
  }

  public async Task Consume(ConsumeContext<ProductCheckedOut> context)
  {
    var rootTx = _tracer.StartAttachedTransaction(nameof(ProductCheckedOut), TraceConsts.Activities.ProductCheckedOut);
    try
    {
      if (rootTx is not null)
      {
        rootTx.Context.User = new User
        {
          Id = context.Message.UserId
        };
      }

      var entity = _mapper.Map<ProductCheckedOutEvent>(context.Message);
      await _eventRepository.CreateAsync(entity, context.CancellationToken);
      await _unitOfWork.SaveEntitiesAsync(context.CancellationToken);

      await _taskHubCtx.Clients.User(entity.UserId).ProductCheckOutSucceeded(context.Message);
    }
    catch (Exception exc)
    {
      rootTx?.CaptureException(exc);
    }
    finally
    {
      rootTx?.End();
    }
  }
}