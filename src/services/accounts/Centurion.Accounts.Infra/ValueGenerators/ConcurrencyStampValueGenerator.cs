using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Centurion.Accounts.Infra.ValueGenerators;

public class ConcurrencyStampValueGenerator : ValueGenerator<string>
{
  public override bool GeneratesTemporaryValues => false;

  public override string Next(EntityEntry entry)
  {
    return Guid.NewGuid().ToString("N").ToUpperInvariant();
  }
}