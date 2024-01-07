using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Centurion.Contracts;

namespace Centurion.Cli.Core.Services.Modules.Accessors;

public abstract class PrimitiveFieldAccessor<T> : IConfigFieldAccessor<T>
{
  private readonly BehaviorSubject<T?> _source;

  protected PrimitiveFieldAccessor(BehaviorSubject<T?> source, ConfigFieldDescriptor descriptor)
  {
    Descriptor = descriptor;
    _source = source;

    Source = source;
  }

  public CompositeDisposable Disposable { get; } = new();
  public void SetValue(object? value) => _source.OnNext((T?) value);

  public object? GetValue() => _source.Value;

  public ConfigFieldDescriptor Descriptor { get; }
  public ISubject<T?> Source { get; }

  public void Dispose()
  {
    _source.Dispose();
    Disposable.Dispose();
  }
}