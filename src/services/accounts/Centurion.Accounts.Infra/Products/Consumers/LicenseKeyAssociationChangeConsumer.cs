using Discord;
using MassTransit;
using Centurion.Accounts.Core.Identity;
using Centurion.Accounts.Core.Identity.Services;
using Centurion.Accounts.Core.Products.Events.LicenseKeys;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Products.Consumers;

public class LicenseKeyAssociationChangeConsumer
  : IConsumer<LicenseKeyRemoved>, IConsumer<LicenseKeyPurchased>, IConsumer<LicenseKeyUnbound>,
    IConsumer<LicenseKeyBound>
{
  private readonly ILicenseKeyRepository _licenseKeyRepository;
  private readonly IPlanRepository _planRepository;
  private readonly IDiscordClientProvider _discordClientProvider;
  private readonly IUserRepository _userRepository;

  public LicenseKeyAssociationChangeConsumer(ILicenseKeyRepository licenseKeyRepository,
    IPlanRepository planRepository, IDiscordClientProvider discordClientProvider, IUserRepository userRepository)
  {
    _licenseKeyRepository = licenseKeyRepository;
    _planRepository = planRepository;
    _discordClientProvider = discordClientProvider;
    _userRepository = userRepository;
  }

  public async Task Consume(ConsumeContext<LicenseKeyRemoved> context) =>
    await HandleAssociationChangeAsync(context.Message, false, context.CancellationToken);

  public async Task Consume(ConsumeContext<LicenseKeyUnbound> context) =>
    await HandleAssociationChangeAsync(context.Message, false, context.CancellationToken);

  public async Task Consume(ConsumeContext<LicenseKeyPurchased> context) =>
    await HandleAssociationChangeAsync(context.Message, true, context.CancellationToken);

  public async Task Consume(ConsumeContext<LicenseKeyBound> context) =>
    await HandleAssociationChangeAsync(context.Message, true, context.CancellationToken);

  private async Task HandleAssociationChangeAsync(LicenseKeyAssociationChange associationChange,
    bool isDisassociated, CancellationToken ct)
  {
    IReadOnlyCollection<long> exceptKeys = Array.Empty<long>();
    if (isDisassociated)
    {
      exceptKeys = new[] {associationChange.LicenseKeyId};
    }

    if (!associationChange.UserId.HasValue || await _licenseKeyRepository.ExistsForPlanAsync(
          associationChange.UserId.GetValueOrDefault(), associationChange.PlanId, exceptKeys, ct))
    {
      return;
    }

    (ulong guildId, IEnumerable<ulong> roleIds) =
      await _planRepository.GetDiscordRolesInfoAsync(associationChange.PlanId, ct);
    User? user = await _userRepository.GetByIdAsync(associationChange.UserId.Value, ct);

    var client = await _discordClientProvider.GetInitializedClientAsync(associationChange.DashboardId, ct);
    var guild = client.Guilds.FirstOrDefault(_ => _.Id == guildId);
    if (guild == null)
    {
      return;
    }

    var member = guild.GetUser(user!.DiscordId);
    var roles = member.Roles.Where(_ => roleIds.Contains(_.Id));
    var options = new RequestOptions
    {
      AuditLogReason = "API",
      CancelToken = ct
    };

    if (isDisassociated)
    {
      await member.RemoveRolesAsync(roles, options);
    }
    else
    {
      await member.AddRolesAsync(roles, options);
    }
  }
}