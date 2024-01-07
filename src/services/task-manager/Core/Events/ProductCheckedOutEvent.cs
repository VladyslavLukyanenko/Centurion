using Centurion.SeedWork.Primitives;
using NodaTime;

namespace Centurion.TaskManager.Core.Events;

public class ProductCheckedOutEvent : Entity, IUserProperty
{
  // public Guid Id { get; init; }
  public string UserId { get; init; } = null!;
  public Instant Timestamp { get; set; }

  public string Title { get; init; } = null!;
  public string Picture { get; init; } = null!;
  public decimal Price { get; init; }
  public string FormattedPrice { get; set; } = default!;
  public string Thumbnail { get; init; } = null!;
  public string Url { get; init; } = null!;
  public string Mode { get; init; } = null!;
  public uint Qty { get; init; }
  public Duration Delay { get; init; }
  public string Profile { get; init; } = null!;
  public string Store { get; init; } = null!;
  public IList<ProductAttr> Attrs { get; init; } = new List<ProductAttr>();
  public IList<string> ProcessingLog { get; init; } = new List<string>();
  public string TaskId { get; init; } = null!;
  public string ShopIconUrl { get; init; } = null!;
  public string ShopTitle { get; init; } = null!;
  public IList<ProductLink> Links { get; init; } = new List<ProductLink>();
  public string Sku { get; init; } = null!;
  public string? Account { get; init; }
  public Duration Duration { get; init; }
  public string? Proxy { get; init; }
}