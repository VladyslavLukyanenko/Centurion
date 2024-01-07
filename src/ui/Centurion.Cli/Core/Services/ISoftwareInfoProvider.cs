namespace Centurion.Cli.Core.Services;

public interface ISoftwareInfoProvider
{
  IObservable<string> SoftwareVersion { get; }
  string CurrentSoftwareVersion { get; }
  void SetSoftwareVersion(string version);
}