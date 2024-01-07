using Centurion.Cli.Core.Services.Modules.Accessors;
using Centurion.Contracts;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Centurion.Cli.Core.Services.Modules;

public interface IFieldAccessorFactory
{
  IConfigFieldAccessor Create(ConfigFieldDescriptor descriptor, IFieldAccessor fieldAccessor,
    IMessage instance);
}