using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Centurion.Contracts;

namespace Centurion.Cli.Core.Services.Modules.Accessors;

public interface IConfigFieldAccessor : IDisposable
{
  CompositeDisposable Disposable { get; }
  void SetValue(object? value);
  object? GetValue();
  ConfigFieldDescriptor Descriptor { get; }
}

public interface IConfigFieldAccessor<T> : IConfigFieldAccessor
{
  ISubject<T?> Source { get; }
}