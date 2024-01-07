using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Contracts;
using Google.Protobuf;

namespace Centurion.Cli.Core.Services.Modules;

public interface IModuleReflector
{
  IMessage CreateEmpty(ConfigDescriptor descriptor);
  CheckoutModeMetadata? GetSelectedModeOrDefault(IMessage moduleConfig, ModuleMetadata meta);

  IReadOnlyList<IConfigFieldAccessor> CreateFieldAccessors(ConfigDescriptor descriptor,
    IMessage instance);

  IMessage Create(ConfigDescriptor config, byte[] rawConfig);
  IMessage GetOrSet(IMessage moduleConfig, ConfigDescriptor descriptor);
}