using Centurion.Accounts.Infra.Converters;

namespace Centurion.Accounts.Infra.Services;

public abstract class DataConverterBase<TSource, TDst> : IDataConverter<TSource, TDst>
{
  public async Task<TDst> ConvertAsync(TSource src, CancellationToken ct = default)
  {
    var list = await ConvertAsync(new[] {src}, ct);
    return list[0];
  }

  public abstract Task<IList<TDst>> ConvertAsync(IEnumerable<TSource> src, CancellationToken ct = default);
}