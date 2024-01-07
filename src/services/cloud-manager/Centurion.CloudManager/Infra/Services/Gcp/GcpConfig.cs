namespace Centurion.CloudManager.Infra.Services.Gcp;

public class GcpConfig
{
  public string SourceInstanceTemplate { get; init; } = null!;
  public string Zone { get; init; } = null!;
  public string ProjectId { get; init; } = null!;
  public string CredentialsPath { get; init; } = null!;
  public string ApplicationName { get; init; } = null!;
  public string MachineType { get; init; } = null!;
}