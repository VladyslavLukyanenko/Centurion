namespace MonitoringFakeShop;

public class Product
{
  public static Product DemoProduct { get; } = new()
  {
    Id = ProductId,
    IsAvailable = false,
    Name = "Fake Product",
    Price = 9.99M,
    ProductPic = "https://unsplash.com/photos/RnCPiXixooY",
    Index = int.MinValue
  };
    
  private static readonly Guid ProductId = Guid.NewGuid();

  public string ProductPic { get; set; }
  public string Name { get; set; }
  public Guid Id { get; set; }
  public bool IsAvailable { get; set; }
  public decimal Price { get; set; }
  public int Index { get; set; }
}