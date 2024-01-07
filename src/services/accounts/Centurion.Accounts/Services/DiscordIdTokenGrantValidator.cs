using Centurion.Accounts.App.Services.Discord;
using Centurion.Accounts.Core.Products.Services;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Centurion.Accounts.Services;

public class DiscordIdTokenGrantValidator : IExtensionGrantValidator
{
  private readonly IDiscordClient _discordClient;
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IDiscordAuthenticationHandler _authenticationHandler;
  public const string GrantTypeName = "discord-token.auth";

  public DiscordIdTokenGrantValidator(IDiscordClient discordClient, IDashboardRepository dashboardRepository,
    IDiscordAuthenticationHandler authenticationHandler)
  {
    _discordClient = discordClient;
    _dashboardRepository = dashboardRepository;
    _authenticationHandler = authenticationHandler;
  }

  public async Task ValidateAsync(ExtensionGrantValidationContext context)
  {
    var token = context.Request.Raw["code"];
    if (string.IsNullOrEmpty(token))
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Token is empty");
      return;
    }

    var rawDashboardLocation = context.Request.Raw["dashboard"];
    if (string.IsNullOrEmpty(rawDashboardLocation))
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "No dashboard url provided");
      return;
    }

    var dashboard = await _dashboardRepository.GetByRawLocationAsync(rawDashboardLocation);
    if (dashboard == null)
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Dashboard not found");
      return;
    }

    var securityToken = await _discordClient.AuthenticateAsync(dashboard.DiscordConfig.OAuthConfig, token);
    if (securityToken == null)
    {
      context.Result =
        new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Discord rejected authentication attempt");
      return;
    }

    var result = await _authenticationHandler.AuthenticateAsync(dashboard, securityToken);
    if (result.IsFailure)
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, result.Error);
      return;
    }

    context.Result = new GrantValidationResult(result.Value.AuthenticatedUser.Id.ToString(), GrantType,
      result.Value.Claims);
  }

  public string GrantType => GrantTypeName;
}