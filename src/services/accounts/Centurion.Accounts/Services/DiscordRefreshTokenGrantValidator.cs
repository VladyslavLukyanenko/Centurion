using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Centurion.Accounts.App.Services.Discord;
using Centurion.Accounts.Core.Products.Services;
using Centurion.Accounts.Foundation.Authorization;
using CSharpFunctionalExtensions;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecurityToken = Centurion.Accounts.App.Model.Discord.SecurityToken;

namespace Centurion.Accounts.Services;

public class DiscordRefreshTokenGrantValidator : IExtensionGrantValidator
{
  private readonly IDiscordClient _discordClient;
  private readonly JwtSecurityTokenHandler _securityTokenHandler;
  private readonly TokenValidationParameters _tokenValidationParameters;
  private readonly IDashboardRepository _dashboardRepository;
  private readonly IDiscordAuthenticationHandler _authenticationHandler;
  private readonly ISigningCredentialStore _signingCredentialStore;
  public const string GrantTypeName = "discord-token.refresh";

  public DiscordRefreshTokenGrantValidator(IDiscordClient discordClient, JwtSecurityTokenHandler securityTokenHandler,
    IOptions<TokenValidationParameters> validationParamsOptions, IDashboardRepository dashboardRepository,
    IDiscordAuthenticationHandler authenticationHandler, ISigningCredentialStore signingCredentialStore)
  {
    _discordClient = discordClient;
    _securityTokenHandler = securityTokenHandler;
    _dashboardRepository = dashboardRepository;
    _authenticationHandler = authenticationHandler;
    _signingCredentialStore = signingCredentialStore;
    _tokenValidationParameters = validationParamsOptions.Value;
  }

  public async Task ValidateAsync(ExtensionGrantValidationContext context)
  {
    var token = context.Request.Raw["refresh_token"];
    var rawAccessToken = context.Request.Raw["access_token"];
    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(rawAccessToken))
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Token is empty");
      return;
    }

    var credentials = await _signingCredentialStore.GetSigningCredentialsAsync();
    var validationParams = _tokenValidationParameters.Clone();
    validationParams.ValidateLifetime = false;
    validationParams.IssuerSigningKey = credentials.Key;

    var principal =
      _securityTokenHandler.ValidateToken(rawAccessToken, validationParams, out var validatedAccessToken);
    if (validatedAccessToken == null)
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid access token");
      return;
    }

    var dashboardId = principal.GetDashboardId().GetValueOrDefault();
    var dashboard = await _dashboardRepository.GetByIdAsync(dashboardId);
    if (dashboard == null)
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Dashboard not found");
      return;
    }

    var expiresIn = GetValue(principal, AppClaimNames.DiscordExpiresIn,
      c => TimeSpan.FromSeconds(long.Parse(c.Value)));
    var accessToken = GetValue(principal, AppClaimNames.DiscordAccessTokenToken);
    var refreshToken = GetValue(principal, AppClaimNames.DiscordRefreshTokenToken);
    var refreshedAt = GetValue(principal, AppClaimNames.DiscordRefreshedAt,
      d => DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(d.Value)));
    if (expiresIn.HasNoValue || accessToken.HasNoValue || refreshToken.HasNoValue || refreshedAt.HasNoValue)
    {
      context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Token is invalid");
      return;
    }

    SecurityToken? securityToken;
    if (!IsDiscordTokenExpiresSoon(expiresIn.Value, refreshedAt.Value))
    {
      securityToken = new SecurityToken((int) expiresIn.Value.TotalSeconds, accessToken.Value, refreshToken.Value);
    }
    else
    {
      securityToken = await _discordClient.ReauthenticateAsync(dashboard.DiscordConfig.OAuthConfig, token);
    }

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

  private bool IsDiscordTokenExpiresSoon(TimeSpan expiresIn, DateTimeOffset issuedAt)
  {
    var half = expiresIn / 2;
    var elapsed = DateTimeOffset.UtcNow - issuedAt;
    return elapsed >= half;
  }

  public string GrantType => GrantTypeName;

  private Maybe<string> GetValue(ClaimsPrincipal t, string claimType) => GetValue(t, claimType, _ => _.Value);

  private Maybe<T> GetValue<T>(ClaimsPrincipal t, string claimType, Func<Claim, T> projector) =>
    t.Claims.TryFirst(c => c.Type == claimType).Select(projector);
}