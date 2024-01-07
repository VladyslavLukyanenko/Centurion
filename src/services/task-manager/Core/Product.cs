using Centurion.Contracts;

namespace Centurion.TaskManager.Core;

public class Product
{
  private Product()
  {
  }

  public Product(Module module, string sku)
  {
    Sku = sku;
    Module = module;
  }

  public CompositeId GetCompositeId() => new(Module, Sku);
  public string Sku { get; init; } = null!;
  public string Name { get; init; } = null!;
  public string Image { get; init; } = null!;
  public string Link { get; init; } = null!;
  public Module Module { get; init; }
  public decimal? Price { get; init; }

  public readonly struct CompositeId : IEquatable<CompositeId>
  {
    public const string Delim = "__";
    public CompositeId(Module module, string sku)
    {
      Module = module;
      Sku = sku;
    }

    public Module Module { get; init; }
    public string Sku { get; init; }

    public override string ToString()
    {
      return $"{Module}{Delim}{Sku}";
    }

    public bool Equals(CompositeId other)
    {
      return Module == other.Module && Sku == other.Sku;
    }

    public override bool Equals(object? obj)
    {
      return obj is CompositeId other && Equals(other);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Module, Sku);
    }

    public static bool operator ==(CompositeId left, CompositeId right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(CompositeId left, CompositeId right)
    {
      return !left.Equals(right);
    }
  }
}