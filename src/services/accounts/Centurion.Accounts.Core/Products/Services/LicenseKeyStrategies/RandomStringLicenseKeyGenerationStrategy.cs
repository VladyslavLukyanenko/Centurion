using System.Text;

namespace Centurion.Accounts.Core.Products.Services.LicenseKeyStrategies;

public class RandomStringLicenseKeyGenerationStrategy : ILicenseKeyGenerationStrategy
{
  private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  private static readonly Random Rnd = new((int) DateTime.Now.Ticks);
  private const int KeyLength = 32;
  private readonly ILicenseKeyRepository _licenseKeyRepository;

  public RandomStringLicenseKeyGenerationStrategy(ILicenseKeyRepository licenseKeyRepository)
  {
    _licenseKeyRepository = licenseKeyRepository;
  }

  public bool IsSupported(LicenseKeyGeneratorConfig config) => config.Format == LicenseKeyFormat.RandomString;

  public async ValueTask<string> GenerateValueAsync(Plan plan, CancellationToken ct = default)
  {
    string key;
    do
    {
      key = MakeRandom(KeyLength);
    } while (await _licenseKeyRepository.ExistsWithValueAsync(key, ct));

    return key;
  }

  private string MakeRandom(int length)
  {
    var sb = new StringBuilder("", length);
    for (int i = 0; i < length; i++)
    {
      var c = Rnd.Next(0, Characters.Length - 1);
      sb.Append(c);
    }

    return sb.ToString();
  }
}