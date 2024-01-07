namespace Centurion.SeedWork.Infra.EfCoreNpgsql.Services;

public interface IDataSeeder
{
  int Order { get; }

  Task SeedAsync();
}