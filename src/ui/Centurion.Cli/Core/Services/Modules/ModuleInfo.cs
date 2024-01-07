using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Centurion.Cli.Core.Services.Modules;

internal class ModuleInfo
{
  public MessageDescriptor Descriptor { get; init; } = null!;
  public MessageParser Parser { get; init; } = null!;
}