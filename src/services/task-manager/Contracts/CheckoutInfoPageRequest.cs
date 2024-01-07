using Centurion.TaskManager.Application.Services.Analytics;
using NodaTime;
using NodaTime.Extensions;

// ReSharper disable once CheckNamespace
namespace Centurion.Contracts.Analytics;

public partial class CheckoutInfoPageRequest : ICheckoutInfoPageRequest
{
  Instant ICheckoutInfoPageRequest.StartAt => StartAt.ToDateTimeOffset().ToInstant();
  Instant ICheckoutInfoPageRequest.EndAt => EndAt.ToDateTimeOffset().ToInstant();

  public string? OrderBy => null;
  public int Offset => Limit * PageIndex;
  public bool IsOrdered => true;
  
  public string NormalizeSearchTerm() => SearchTerm?.ToUpperInvariant() ?? "";
  public bool IsSearchTermEmpty() => string.IsNullOrWhiteSpace(SearchTerm);
}