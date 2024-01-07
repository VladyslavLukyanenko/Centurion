using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Centurion.Cli.Core.Services;

public class SoftwareInfoProvider : ISoftwareInfoProvider
{
  private readonly BehaviorSubject<string> _softwareVersion = new BehaviorSubject<string>(string.Empty);

  public SoftwareInfoProvider(ILicenseKeyProvider licenseKeyProvider)
  {
    SoftwareVersion = _softwareVersion.AsObservable()
      .CombineLatest(licenseKeyProvider.LicenseKey, (softV, key) => string.IsNullOrEmpty(key) ? string.Empty : softV);
  }

  public IObservable<string> SoftwareVersion { get; }
  public string CurrentSoftwareVersion => _softwareVersion.Value;

  public void SetSoftwareVersion(string version)
  {
    _softwareVersion.OnNext(version);
  }
}