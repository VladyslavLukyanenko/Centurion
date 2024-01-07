using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Data;

public class ConcurrencyStampValueGenerator : ValueGenerator<string>
{
  public override bool GeneratesTemporaryValues => false;

  public override string Next(EntityEntry entry)
  {
    return Guid.NewGuid().ToString("N").ToUpperInvariant();
  }
}