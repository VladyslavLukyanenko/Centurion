using Centurion.Accounts.Core.Primitives;
using CSharpFunctionalExtensions;

namespace Centurion.Accounts.Core.Products;

public class Release : DashboardBoundEntity, IConcurrentEntity
{
  private int _stock;
  private int _initialStock;

  private Release()
  {
  }

  public Release(string password, int stock, string title, ReleaseType type, long planId, Guid dashboardId,
    bool isActive)
    : base(dashboardId)
  {
    Password = password;
    Title = title;
    Type = type;
    _stock = stock;
    InitialStock = stock;
    PlanId = planId;
    IsActive = isActive;
  }

  public string Password { get; set; } = null!;

  public int InitialStock
  {
    get => _initialStock;
    set
    {
      _initialStock = value;
      _stock = Math.Min(_stock, value);
    }
  }

  public ReleaseType Type { get; set; }
  public int Stock => _stock;
  public string Title { get; set; } = null!;
  public bool IsActive { get; set; }
  public long PlanId { get; set; }
  public string? ConcurrencyStamp { get; private set; }
    
  public Result Decrement()
  {
    var result = Interlocked.Decrement(ref _stock);
    if (result < 0)
    {
      _stock = 0;
      return Result.Failure("Out of stock");
    }

    return Result.Success();
  }
}