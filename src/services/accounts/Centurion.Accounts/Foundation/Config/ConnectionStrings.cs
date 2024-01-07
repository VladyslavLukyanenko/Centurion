namespace Centurion.Accounts.Foundation.Config;

public class ConnectionStrings
{
  public string Npgsql { get; init; } = null!;
  public string RabbitMq { get; init; } = null!;
}