using AutoMapper;
using Centurion.Contracts.Analytics;
using Centurion.SeedWork.Web;
using Centurion.TaskManager.Application.Services.Analytics;
using Grpc.Core;
using NodaTime;
using static Centurion.Contracts.Analytics.Analytics;
using CheckoutInfoPageRequest = Centurion.Contracts.Analytics.CheckoutInfoPageRequest;

namespace Centurion.TaskManager.Web.Grpc;

public class AnalyticsService : AnalyticsBase
{
  private readonly IAnalyticsProvider _analyticsProvider;
  private readonly IMapper _mapper;

  public AnalyticsService(IAnalyticsProvider analyticsProvider, IMapper mapper)
  {
    _analyticsProvider = analyticsProvider;
    _mapper = mapper;
  }

  public override async Task<CheckoutInfoPagedList> GetCheckoutInfoPage(CheckoutInfoPageRequest request,
    ServerCallContext context)
  {
    var page = await _analyticsProvider.GetCheckoutsPage(context.GetUserId(), request, context.CancellationToken);

    return _mapper.Map<CheckoutInfoPagedList>(page);
  }

  public override async Task<AnalyticsSummary> GetSummary(AnalyticsSummaryRequest request, ServerCallContext context)
  {
    var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(request.TimeZone);
    if (timeZone is null)
    {
      throw new RpcException(new Status(StatusCode.InvalidArgument, "No valid zone provided"));
    }

    return await _analyticsProvider.GetSummary(context.GetUserId(), timeZone, context.CancellationToken);
  }
}