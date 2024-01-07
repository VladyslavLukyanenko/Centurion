namespace Centurion.Accounts.Infra;

public interface IDataSeeder
{
  int Order { get; }

  Task SeedAsync();
}