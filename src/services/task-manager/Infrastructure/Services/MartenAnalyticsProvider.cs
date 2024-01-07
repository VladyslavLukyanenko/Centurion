// using AutoMapper;
// using Centurion.Accounts.Core.Collections;
// using Centurion.Accounts.Infra;
// using Centurion.Contracts.Analytics;
// using Centurion.TaskManager.Application.Services.Analytics;
// using Centurion.TaskManager.Core.Events;
// using Marten;
//
// namespace Centurion.TaskManager.Infrastructure.Services;
//
// public class MartenAnalyticsProvider : IAnalyticsProvider
// {
//   private readonly IDocumentStore _store;
//   private readonly IMapper _mapper;
//
//   public MartenAnalyticsProvider(IDocumentStore store, IMapper mapper)
//   {
//     _store = store;
//     _mapper = mapper;
//   }
//
//   public async ValueTask<IPagedList<CheckoutInfoData>> GetCheckoutsPage(ICheckoutInfoPageRequest request,
//     CancellationToken ct = default)
//   {
//     await using var session = _store.LightweightSession();
//     var page = await session.Query<ProductCheckedOutEvent>()
//       .PaginateAsync(request, ct);
//
//     return page.ProjectTo(_mapper.Map<CheckoutInfoData>);
//   }
// }