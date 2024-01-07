namespace Centurion.Accounts.Infra.Converters;

public interface IDataConverter<in TSource, TDst>
{
  Task<TDst> ConvertAsync(TSource src, CancellationToken ct = default);
  Task<IList<TDst>> ConvertAsync(IEnumerable<TSource> src, CancellationToken ct = default);
}