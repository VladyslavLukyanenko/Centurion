using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Centurion.Accounts.Infra;

public static class DbContextExtensions
{
  // ReSharper disable once CognitiveComplexity
  public static void UseSnakeCaseNamingConvention(this ModelBuilder modelBuilder)
  {foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
      // Replace table names
      // todo: fix it when not only TPH will be supported
      if (string.IsNullOrEmpty(entity.BaseType?.GetRootType().ShortName()))
      {
        entity.SetTableName(entity.GetTableName().ToSnakeCase());
        entity.SetSchema(entity.GetSchema().ToSnakeCase());
      }

      var storeObjId = StoreObjectIdentifier.Table(entity.GetTableName(), entity.GetSchema());
      // Replace column names            
      foreach (var property in entity.GetProperties())
      {
        if (property.IsShadowProperty()) continue;

        var name = property.GetColumnName(storeObjId).ToSnakeCase();
        property.SetColumnName(name);
      }

      foreach (var key in entity.GetKeys())
      {
        key.SetName(key.GetName().ToSnakeCase());
      }

      foreach (var key in entity.GetForeignKeys())
      {
        key.SetConstraintName(key.GetDefaultName().ToSnakeCase());
      }

      foreach (var index in entity.GetIndexes())
      {
        index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
      }
    }
  }
}