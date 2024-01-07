using Bogus;

namespace MonitoringFakeShop;

public class InMemRepo
{
  private static readonly SemaphoreSlim Gates = new(1, 1);
  public static List<Product> Products = new();

  static InMemRepo()
  {
    var idx = 0;
    var faker = new Faker<Product>()
      .RuleFor(_ => _.Id, Guid.NewGuid)
      .RuleFor(_ => _.Name, _ => _.Commerce.ProductName())
      .RuleFor(_ => _.Price, _ => Math.Round(_.Random.Decimal(10, 300), 2))
      .RuleFor(_ => _.ProductPic, _ => _.Image.PicsumUrl())
      .RuleFor(_ => _.IsAvailable, _ => _.Random.Bool(.25f));

    var count = 200;
    Products.AddRange(faker.Generate(count));
    foreach (var product in Products)
    {
      product.Index = idx++;
    }

    Products = Products.OrderBy(_ => _.Index).ToList();
  }

  public static void Update(Product newProduct)
  {
    try
    {
      Gates.Wait();

      var toReplaceIdx = Products.FindIndex(_ => _.Id == newProduct.Id);
      Products.RemoveAt(toReplaceIdx);
      Products.Insert(toReplaceIdx, newProduct);

      Products = Products.OrderBy(_ => _.Index).ToList();
    }
    finally
    {
      Gates.Release();
    }
  }
}