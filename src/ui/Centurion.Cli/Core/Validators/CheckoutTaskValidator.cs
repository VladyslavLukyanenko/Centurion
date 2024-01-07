using Centurion.Cli.Core.Domain.Tasks;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Services.Sessions;
using Centurion.Contracts;
using FluentValidation;

namespace Centurion.Cli.Core.Validators;

public class CheckoutTaskValidator : AbstractValidator<CheckoutTaskModel>
{
  private readonly IProfilesRepository _profilesRepository;
  private readonly ISessionRepository _sessionRepository;
  private readonly IProxyGroupsRepository _proxyGroupsRepository;
  private readonly ISessionConfigurationQueue _sessionConfigQueue;

  public CheckoutTaskValidator(IProfilesRepository profilesRepository, ISessionRepository sessionRepository,
    IProxyGroupsRepository proxyGroupsRepository, ISessionConfigurationQueue sessionConfigQueue)
  {
    _profilesRepository = profilesRepository;
    _sessionRepository = sessionRepository;
    _proxyGroupsRepository = proxyGroupsRepository;
    _sessionConfigQueue = sessionConfigQueue;

    RuleFor(_ => _.ProductSku).NotEmpty().WithMessage("Sku empty");

    RuleFor(_ => _.ProfileIds).Must(BeValidProfile).WithMessage("Profile not selected");

    /*RuleFor(_ => _.SessionId)
      .Must(BeNonQueuedForConfiguration)
      .When(_ => _.SessionId.HasValue)
      .WithMessage("Selected session queued for config")
        
      .Must(BeValidSession)
      .When(_ => _.SessionId.HasValue)
      .WithMessage("Session not selected");*/

    RuleFor(_ => _.CheckoutProxyPoolId)
      .Must(BeValidProxy)
      .When(_ => _.CheckoutProxyPoolId.HasValue)
      .WithMessage("Checkout proxy not selected");

    RuleFor(_ => _.MonitorProxyPoolId)
      .Must(BeValidProxy)
      .When(_ => _.MonitorProxyPoolId.HasValue)
      .WithMessage("Monitor proxy not selected");

    RuleFor(_ => _.Module).IsInEnum().WithMessage("Module empty");
  }

  private bool BeNonQueuedForConfiguration(Guid? sessionId)
  {
    if (!sessionId.HasValue)
    {
      return true;
    }

    var session = _sessionRepository.LocalItems.FirstOrDefault(_ => _.Id == sessionId);
    return session is null || !_sessionConfigQueue.IsQueued(session);
  }

  private bool BeValidProxy(Guid? proxyId)
  {
    if (!proxyId.HasValue)
    {
      return true;
    }

    return _proxyGroupsRepository.LocalItems.Any(_ => _.Id == proxyId);
  }

  private bool BeValidSession(Guid? sessionId)
  {
    if (!sessionId.HasValue)
    {
      return true;
    }

    var session = _sessionRepository.LocalItems.FirstOrDefault(_ => _.Id == sessionId);

    return session?.Status == SessionStatus.Ready;
  }

  private bool BeValidProfile(ISet<Guid> profileIds)
  {
    var exists = profileIds.All(profileId =>
      _profilesRepository.LocalItems.Any(_ => _.Profiles.Any(p=> p.Id == profileId)));
    return exists;
  }
}