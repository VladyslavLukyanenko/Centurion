using AutoMapper;
using Centurion.Accounts.App.Products.Services;
using Centurion.Accounts.Core.WebHooks;
using Centurion.Accounts.Core.WebHooks.Services;

namespace Centurion.Accounts.App.WebHooks;

public abstract class WebHookMapperBase<TData> : IWebHookPayloadMapper
  where TData : WebHookDataBase
{
  private readonly IMapper _mapper;
  private readonly IDashboardRefProvider _dashboardRefProvider;

  private readonly IWebHookPayloadFactory _webHookPayloadFactory;
  private readonly IWebHookBindingRepository _webHookBindingRepository;

  protected WebHookMapperBase(IMapper mapper, IDashboardRefProvider dashboardRefProvider,
    IWebHookPayloadFactory webHookPayloadFactory, IWebHookBindingRepository webHookBindingRepository)
  {
    _mapper = mapper;
    _dashboardRefProvider = dashboardRefProvider;
    _webHookPayloadFactory = webHookPayloadFactory;
    _webHookBindingRepository = webHookBindingRepository;
  }

  public abstract bool CanMap(object @event);

  public async ValueTask<WebHookPayloadEnvelop?> MapAsync(object @event, CancellationToken ct = default)
  {
    var data = _mapper.Map<TData>(@event);
    var binding = await _webHookBindingRepository.GetByTypeAsync(data.Dashboard.Id, data.Type, ct);
    if (binding == null)
    {
      return null;
    }

    await _dashboardRefProvider.InitializeAsync(data.Dashboard, ct);
    await InitializeAsync(data, ct);

    return await _webHookPayloadFactory.CreateAsync(binding, data, ct);
  }

  protected virtual ValueTask InitializeAsync(TData data, CancellationToken ct) => ValueTask.CompletedTask;
}