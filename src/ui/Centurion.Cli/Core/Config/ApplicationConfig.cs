namespace Centurion.Cli.Core.Config;

public class ApplicationConfig
{
  public string StorageLocation { get; init; } = null!;
  public ConnectionStringsConfig ConnectionStrings { get; init; } = new();
  public SecurityConfig Security { get; init; } = new();
  public ClientsConfig ClientsConfig { get; init; } = new();
  public HarvesterConfig HarvesterConfig { get; init; } = new();
  public GeneralSettingsConfig General { get; init; } = new();
}