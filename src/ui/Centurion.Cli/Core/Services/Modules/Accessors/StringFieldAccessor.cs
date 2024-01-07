using System.Reactive.Subjects;
using Centurion.Contracts;

namespace Centurion.Cli.Core.Services.Modules.Accessors;

public class StringFieldAccessor : PrimitiveFieldAccessor<string>
{
  public StringFieldAccessor(BehaviorSubject<string?> source, ConfigFieldDescriptor descriptor)
    : base(source, descriptor)
  {
  }
}