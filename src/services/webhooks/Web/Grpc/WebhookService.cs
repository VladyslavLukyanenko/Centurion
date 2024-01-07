using Centurion.Contracts;
using Centurion.Contracts.Webhooks;
using Centurion.SeedWork.Primitives;
using Centurion.SeedWork.Web;
using Centurion.WebhookSender.Core;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Centurion.WebhookSender.Web.Grpc;

[Authorize]
public class WebhookService : Webhooks.WebhooksBase
{
  private readonly IWebhookSettingsProvider _provider;
  private readonly IWebhookSettingsRepository _repository;
  private readonly IUnitOfWork _unitOfWork;

  public WebhookService(IWebhookSettingsProvider provider, IWebhookSettingsRepository repository,
    IUnitOfWork unitOfWork)
  {
    _provider = provider;
    _repository = repository;
    _unitOfWork = unitOfWork;
  }

  public override async Task<WebhookSettings> GetSettings(Empty request, ServerCallContext context)
  {
    var settings = await _provider.GetSettingsForUserAsync(context.GetUserId(), context.CancellationToken);
    if (settings == null)
    {
      throw new RpcException(new Status(StatusCode.NotFound, "No settings found"));
    }


    return settings;
  }

  public override async Task<Empty> SaveSettings(SaveSettingsCommand request, ServerCallContext context)
  {
    var ct = context.CancellationToken;
    var settings = await _repository.GetAsync(context.GetUserId(), ct);
    if (settings == null)
    {
      await _repository.CreateAsync(new WebhookSettings
      {
        SuccessUrl = request.SuccessUrl,
        FailureUrl = request.FailureUrl,
        UserId = context.GetUserId()
      }, ct);
    }
    else
    {
      settings.SuccessUrl = request.SuccessUrl;
      settings.FailureUrl = request.FailureUrl;

      _repository.Update(settings);
    }

    await _unitOfWork.SaveChangesAsync(ct);

    return new();
  }
}