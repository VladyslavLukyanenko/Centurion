using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NodaTime;
using Centurion.Accounts.App;
using Centurion.Accounts.App.Analytics.Model;
using Centurion.Accounts.App.Analytics.Services;
using Centurion.Accounts.Core.Products;
using Centurion.Accounts.Infra.Serialization.Json;

namespace Centurion.Accounts.Infra.Analytics.Services;

public class HttpClientBasedDiscordInsightsClient : IDiscordInsightsClient
{
  private readonly HttpClient _httpClient;

  private static readonly JsonSerializerSettings UnderscoreAwareSerializerSettings;

  static HttpClientBasedDiscordInsightsClient()
  {
    UnderscoreAwareSerializerSettings = new JsonSerializerSettings();
    NewtonsoftJsonSettingsFactory.ConfigureSettingsWithDefaults(UnderscoreAwareSerializerSettings);
    UnderscoreAwareSerializerSettings.ContractResolver = new PascalCaseToUnderscoreContractResolver();
  }

  public HttpClientBasedDiscordInsightsClient(IHttpClientFactory httpClientFactory)
  {
    _httpClient = httpClientFactory.CreateClient(NamedHttpClients.DiscordClient);
  }

  public async ValueTask<IList<JoinBySourceDiscordInsightsData>> GetJoinBySourceAsync(DiscordAnalyticsRequest request,
    DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<JoinBySourceDiscordInsightsData>(request, config, "joins-by-source", ct);

  public async ValueTask<IList<MembershipDiscordInsightsData>> GetMembershipAsync(DiscordAnalyticsRequest request,
    DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<MembershipDiscordInsightsData>(request, config, "membership", ct);

  public async ValueTask<IList<LeaversDiscordInsightsData>> GetLeaversAsync(DiscordAnalyticsRequest request,
    DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<LeaversDiscordInsightsData>(request, config, "leavers", ct);

  public async ValueTask<IList<ActivationDiscordInsightsData>> GetActivationAsync(DiscordAnalyticsRequest request,
    DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<ActivationDiscordInsightsData>(request, config, "activation", ct);

  public async ValueTask<IList<RetentionDiscordInsightsData>> GetRetentionAsync(DiscordAnalyticsRequest request,
    DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<RetentionDiscordInsightsData>(request, config, "retention", ct);

  public async ValueTask<IList<JoinsDiscordInsightsData>> GetJoinsAsync(Instant start, Instant end,
    DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<JoinsDiscordInsightsData>(start, end, DiscordInsightsInterval.Daily, config, "joins",
      ct);

  public async ValueTask<IList<OverviewDiscordInsightsData>> GetOverviewAsync(DiscordAnalyticsRequest request,
    DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<OverviewDiscordInsightsData>(request, config, "overview", ct);

  public async ValueTask<IList<JoinsByInviteDiscordInsightsData>> GetJoinsByInviteAsync(
    DiscordAnalyticsRequest request,
    DiscordConfig config,
    CancellationToken ct = default) =>
    await GetInsightsAsync<JoinsByInviteDiscordInsightsData>(request, config, "joins-by-invite-link", ct);

  public async ValueTask<IList<JoinsByReferrerDiscordInsightsData>> GetJoinsByReferrerAsync(
    DiscordAnalyticsRequest request, DiscordConfig config, CancellationToken ct = default) =>
    await GetInsightsAsync<JoinsByReferrerDiscordInsightsData>(request, config, "joins-by-referrer", ct);

  private async ValueTask<IList<T>> GetInsightsAsync<T>(DiscordAnalyticsRequest request,
    DiscordConfig config, string stats,
    CancellationToken ct = default) =>
    await GetInsightsAsync<T>(request.StartAt, request.EndAt, request.Interval, config, stats, ct);

  private async ValueTask<IList<T>> GetInsightsAsync<T>(Instant startAt, Instant endAt,
    DiscordInsightsInterval interval, DiscordConfig config, string stats,
    CancellationToken ct = default)
  {
    var startAtStr = Uri.EscapeDataString(startAt.ToString());
    var endAtStr = Uri.EscapeDataString(endAt.ToString());
    string url =
      $"https://discord.com/api/v8/guilds/{config.GuildId}/analytics/growth-activation/{stats}?start={startAtStr}&end={endAtStr}&interval={(int) interval}";
    var message = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
    message.Headers.Authorization = new AuthenticationHeaderValue(config.AccessToken!);
    var response = await _httpClient.SendAsync(message, ct);
    var raw = await response.Content.ReadAsStringAsync(ct);

    return JsonConvert.DeserializeObject<IList<T>>(raw, UnderscoreAwareSerializerSettings)!;
  }


  private class PascalCaseToUnderscoreContractResolver : DefaultContractResolver
  {
    protected override string ResolvePropertyName(string propertyName) => propertyName.ToSnakeCase();
  }
}