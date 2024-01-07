using System.Linq.Expressions;
using Centurion.Accounts.Core.Primitives;
using FastExpressionCompiler;

namespace Centurion.Accounts.Core.Audit.Mappings;

public class AuditEntityMappingBuilder<TEntity, TId>
  : IEntityMappingBuilder
  where TEntity : IEntity<TId>
{
  public AuditEntityMappingBuilder(Type? preProcessorType = null)
  {
    PreProcessorType = preProcessorType;
    Mapping = new AuditEntityMapping(o => ((IEntity<TId>) o).Id!.ToString());
    MappedType = typeof(TEntity);
  }

  public AuditEntityMappingBuilder<TEntity, TId> Property(Expression<Func<TEntity, object?>> config,
    Type? converterType = null, PropertyValueAccessor? accessor = null)
  {
    var getter = config.CompileFast();
    var memberExpression = config.Body switch
    {
      MemberExpression me => me,
      UnaryExpression ue => (MemberExpression) ue.Operand,
      _ => throw new NotSupportedException()
    };

    var propName = memberExpression.Member.Name;
    // var info = (PropertyInfo) memberExpression.Member;
    Mapping.Accessors[propName] = accessor ?? ValueAccessor;

    if (converterType != null)
    {
      Mapping.Converters[propName.ToLowerInvariant()] = converterType;
    }

    return this;

    string? ValueAccessor(object o)
    {
      return getter((TEntity) o)?.ToString();
    }
  }

  public Type? PreProcessorType { get; }
  public AuditEntityMapping Mapping { get; }
  public Type MappedType { get; }
}