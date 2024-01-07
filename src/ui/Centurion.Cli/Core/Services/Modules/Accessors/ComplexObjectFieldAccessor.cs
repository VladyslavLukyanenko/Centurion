using System.Reactive.Subjects;
using Centurion.Contracts;

namespace Centurion.Cli.Core.Services.Modules.Accessors;

public class ComplexObjectFieldAccessor : PrimitiveFieldAccessor<object?>
{
  public ComplexObjectFieldAccessor(BehaviorSubject<object?> source, ConfigFieldDescriptor descriptor)
    : base(source, descriptor)
  {
  }
}