using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using FastExpressionCompiler;
using Splat;

namespace Centurion.Cli.Core.Domain.Fields;

public abstract class Field
{
  private readonly BehaviorSubject<object?> _value = new BehaviorSubject<object?>(default);

  private static readonly IDictionary<Type, Func<Field>> Factories = new Dictionary<Type, Func<Field>>();
  private static readonly SemaphoreSlim FactoryGates = new SemaphoreSlim(1, 1);

  protected Field()
  {
    Changed = _value.AsObservable()
      .DistinctUntilChanged()
      .Select(_ => this);
  }

  protected Field(string? systemName, string? displayName, bool isRequired)
    : this()
  {
    SystemName = systemName
                 ?? displayName
                 ?? throw new ArgumentNullException(nameof(systemName), "SystemName is required");
    DisplayName = displayName;
    IsRequired = isRequired;
  }

  public abstract Task<bool> IsValid();

  public virtual IObservable<Field> Changed { get; }

  public string SystemName { get; private set; } = null!;
  public string? DisplayName { get; private set; }
  public bool IsRequired { get; private set; }

  public virtual bool IsEmpty => string.IsNullOrWhiteSpace(ValueId);

  public abstract string? ValueId { get; }
  public abstract string? DisplayValue { get; }

  public object? Value
  {
    get => _value.Value;
    set
    {
      if (AreValueChanged(value))
      {
        return;
      }

      _value.OnNext(value);
    }
  }

  public Field Clone()
  {
    Func<Field>? factory;
    try
    {
      FactoryGates.Wait();
      var type = GetType();
      if (!Factories.TryGetValue(type, out factory))
      {
        ConstructorInfo ctor = type.GetConstructor(new Type[0])
                               ?? throw new InvalidOperationException(
                                 $"Field '{type.Name}' doesn't contains parameterless constructor");
        NewExpression n = Expression.New(ctor);
        Expression<Func<Field>> factoryExpr = Expression.Lambda<Func<Field>>(n);
        factory = factoryExpr.CompileFast();
        Factories[type] = factory;
      }
    }
    finally
    {
      FactoryGates.Release();
    }

    var field = factory();
    CopyTo(field);

    return field;
  }

  public virtual void CopyTo(Field field)
  {
    if (field.GetType() != GetType())
    {
      throw new InvalidOperationException("Field types are different");
    }

    field.SystemName = SystemName;
    field.DisplayName = DisplayName;
    field.IsRequired = IsRequired;

    field.Value = Value;
  }

  protected virtual bool AreValueChanged(object? value)
  {
    return Equals(value, _value.Value);
  }

  public virtual Task InitializeAsync(IReadonlyDependencyResolver dependencyResolver, CancellationToken ct)
  {
    return Task.CompletedTask;
  }

  public override string ToString()
  {
    return $"({GetType().Name})({SystemName}, {nameof(DisplayName)}='{DisplayName}', {nameof(ValueId)}='{ValueId}')";
  }
}