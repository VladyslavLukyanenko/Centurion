namespace Centurion.Accounts.Infra.Services.Cryptographic;

public interface ICryptographicService
{
  Task<string> ComputeHashAsync(Stream stream);
}