using System.Reflection;
using Centurion.Cli.Core;

const string executableName = "Centurion-CLI";
var baseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
var executable = Path.Combine(baseDir!, executableName);
PlatformInteropUtils.Bash($"open \"{executable}\"");