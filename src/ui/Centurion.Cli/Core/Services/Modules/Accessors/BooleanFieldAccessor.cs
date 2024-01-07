using System.Reactive.Subjects;
using Centurion.Contracts;

namespace Centurion.Cli.Core.Services.Modules.Accessors;

public class BooleanFieldAccessor : PrimitiveFieldAccessor<bool>
{
  public BooleanFieldAccessor(BehaviorSubject<bool> source, ConfigFieldDescriptor descriptor)
    : base(source, descriptor)
  {
  }
}