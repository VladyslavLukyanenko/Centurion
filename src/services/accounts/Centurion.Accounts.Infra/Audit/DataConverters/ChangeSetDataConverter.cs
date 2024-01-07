using AutoMapper;
using Centurion.Accounts.App.Audit.Data;
using Centurion.Accounts.App.Identity.Services;
using Centurion.Accounts.Core.Audit;
using Centurion.Accounts.Infra.Services;

namespace Centurion.Accounts.Infra.Audit.DataConverters;

public class ChangeSetDataConverter : DataConverterBase<ChangeSet, ChangeSetData>
{
  private readonly IUserRefProvider _userRefProvider;
  private readonly IMapper _mapper;

  public ChangeSetDataConverter(IUserRefProvider userRefProvider, IMapper mapper)
  {
    _userRefProvider = userRefProvider;
    _mapper = mapper;
  }

  public override async Task<IList<ChangeSetData>> ConvertAsync(IEnumerable<ChangeSet> src,
    CancellationToken ct = default)
  {
    var list = _mapper.Map<List<ChangeSetData>>(src);

    await _userRefProvider.InitializeAsync(list.Select(_ => _.UpdatedBy), ct);

    return list;
  }
}