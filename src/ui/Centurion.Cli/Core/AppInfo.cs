using System.Reflection;

namespace Centurion.Cli.Core;

public class AppInfo
{
  private const string ProductionEnvName = "Production";
#if DEBUG
  private const string FallbackEnvironmentName = "Development";
#else
  private const string FallbackEnvironmentName = ProductionEnvName;
#endif
  public static readonly string EnvironmentName =
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? FallbackEnvironmentName;

  public static Version CurrentAppVersion { get; } = (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly())
    .GetName()
    .Version!;

  public const string ProductTechName = "Centurion.CLI";
  public const string ProductName = "Centurion CLI";

  public static readonly string StorageLocation =
    Path.Combine(EnvironmentHelper.GetLocalAppDataPath(), ProductTechName);

  public static string ProductFullName { get; } = ProductName + " v" + CurrentAppVersion;
  public static bool IsProduction => EnvironmentName == ProductionEnvName;

  public static string InstallationPath { get; } =
    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

#if DEBUG
  public const string ProductId = "825d0121-e5f8-40c3-970d-0134feac1794";
#else
  public const string ProductId = "c215a771-ae8a-46ac-a951-f58cd6651c92";
#endif
}